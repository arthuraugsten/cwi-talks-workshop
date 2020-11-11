using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.IO;
using System.Net.Sockets;

namespace NucleoCompartilhado.EventBus
{
    public sealed class ConexaoPersistentePadraoRabbitMQ : IConexaoPersistenteRabbitMQ
    {
        private readonly object sync_root = new object();
        private readonly IConnectionFactory _conexaoFactory;
        private readonly ILogger<ConexaoPersistentePadraoRabbitMQ> _logger;
        private readonly int _tentarNovamenteContador;

        private IConnection _conexao;
        private bool _disposed;

        public ConexaoPersistentePadraoRabbitMQ(IConnectionFactory conexaoFactory,
             ILogger<ConexaoPersistentePadraoRabbitMQ> logger,
             int tentarNovamenteContador = 5)
        {
            _conexaoFactory = conexaoFactory ?? throw new ArgumentNullException(nameof(conexaoFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tentarNovamenteContador = tentarNovamenteContador;
        }

        public bool EstaConectado
             => _conexao != null && _conexao.IsOpen && !_disposed;

        public IModel CriarModelo()
        {
            if (!EstaConectado)
                throw new InvalidOperationException("Nenhuma conexão com RabbitMQ disponível para executar esta ação");

            return _conexao.CreateModel();
        }

        public bool TentarConectar()
        {
            _logger.LogInformation("Cliente RabbitMQ está tentando se conectar");

            lock (sync_root)
            {
                var politica = Policy.Handle<SocketException>()
                     .Or<BrokerUnreachableException>()
                     .WaitAndRetry(_tentarNovamenteContador, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time)
                          => _logger.LogWarning(ex, "Cliente RabbitMQ não pode se conectar depois de {TimeOut}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message)
                );

                politica.Execute(() => _conexao = _conexaoFactory.CreateConnection());

                if (!EstaConectado)
                {
                    _logger.LogCritical("FATAL ERROR: Conexões do RabbitMQ não puderam serem criadas ou abertas");
                    return false;
                }

                _conexao.ConnectionShutdown += AoDesligarConexao;
                _conexao.CallbackException += NaExcecaoDaChamadaDeRetorno;
                _conexao.ConnectionBlocked += NaConexaoBloqueada;

                _logger.LogInformation("Cliente RabbitMQ adquiriu uma conexão persistente à '{HostName}' e está inscrito para falha de eventos", _conexao.Endpoint.HostName);

                return true;
            }
        }

        private void NaConexaoBloqueada(object sender, ConnectionBlockedEventArgs excecao)
        {
            if (_disposed)
                return;

            _logger.LogWarning("Uma conexão com o RabbitMQ foi desligada. Tentando se reconectar...");

            TentarConectar();
        }

        private void NaExcecaoDaChamadaDeRetorno(object sender, CallbackExceptionEventArgs excecao)
        {
            if (_disposed)
                return;

            _logger.LogWarning("Uma conexão com o RabbitMQ disparou uma exceção. Tentando se reconectar...");

            TentarConectar();
        }

        private void AoDesligarConexao(object sender, ShutdownEventArgs motivo)
        {
            if (_disposed)
                return;

            _logger.LogWarning("Uma conexão com o RabbitMQ foi desligada. Tentando se reconectar...");

            TentarConectar();
        }

        public void Dispose()
        {
            try
            {
                _disposed = true;

                _conexao?.Dispose();
                GC.SuppressFinalize(this);
            }
            catch (IOException ex)
            {
                _logger.LogCritical(ex.ToString());
            }
        }
    }
}