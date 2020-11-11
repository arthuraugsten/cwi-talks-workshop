using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NucleoCompartilhado.EventBus
{
    public sealed class EventBusRabbitMQ : IEventBus, IDisposable
    {
        private const string BrokerName = "cwi_talks_exchange";

        private readonly ConexaoPersistentePadraoRabbitMQ _conexaoPersistente;
        private readonly ILogger<EventBusRabbitMQ> _logger;
        private readonly IGerenciadorAssinaturasEventBus _gerenciadorAssinaturas;
        private readonly IServiceProvider _serviceProvider;
        private readonly int _tentarNovamenteContador;

        private IModel _canalDeConsumo;
        private string _nomeFila;

        public EventBusRabbitMQ(ConexaoPersistentePadraoRabbitMQ conexaoPersistente,
             ILogger<EventBusRabbitMQ> logger,
             IServiceProvider serviceProvider,
             IGerenciadorAssinaturasEventBus gerenciadorAssinaturas,
             string nomeFila = null,
             int tentarNovamenteContador = 5)
        {
            _conexaoPersistente = conexaoPersistente ?? throw new ArgumentNullException(nameof(conexaoPersistente));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _gerenciadorAssinaturas = gerenciadorAssinaturas ?? new GerenciadorAssinaturasEmMemoriaEventBus();
            _nomeFila = nomeFila;
            _canalDeConsumo = CriarCanalConsumoBasico();
            _serviceProvider = serviceProvider;
            _tentarNovamenteContador = tentarNovamenteContador;
            _gerenciadorAssinaturas.AoRemoverEvento += GerenciadorAssinaturas_AposEventoRemovido;
        }

        private void GerenciadorAssinaturas_AposEventoRemovido(object sender, string nomeEvento)
        {
            if (!_conexaoPersistente.EstaConectado)
                _ = _conexaoPersistente.TentarConectar();

            using var canal = _conexaoPersistente.CriarModelo();

            canal.QueueUnbind(queue: _nomeFila, exchange: BrokerName, routingKey: nomeEvento);

            if (_gerenciadorAssinaturas.EstaVazio)
            {
                _nomeFila = string.Empty;
                _canalDeConsumo.Close();
            }
        }

        public void Publicar(EventoIntegracao mensagem)
        {
            if (mensagem == default) throw new ArgumentNullException(nameof(mensagem));

            if (!_conexaoPersistente.EstaConectado)
                _ = _conexaoPersistente.TentarConectar();

            var politica = Policy.Handle<BrokerUnreachableException>()
                 .Or<SocketException>()
                 .WaitAndRetry(_tentarNovamenteContador, tentativa => TimeSpan.FromSeconds(Math.Pow(2, tentativa)), (ex, time) =>
                 {
                     _logger.LogWarning(ex, "Não foi possível publicar o evento: {EventId} depois de {Timeout}s ({ExceptionMessage})", mensagem.EventoId, $"{time.TotalSeconds:n1}", ex.Message);
                 });

            var nomeEvento = mensagem.GetType().Name;

            _logger.LogTrace("Criando canal RabbitMQ para publicar o evento: {EventId} ({EventName})", mensagem.EventoId, nomeEvento);

            using var canal = _conexaoPersistente.CriarModelo();
            _logger.LogTrace("Declarando exchange RabbitMQ para publicar o evento: {EventId}", mensagem.EventoId);

            canal.ExchangeDeclare(exchange: BrokerName, type: "direct");

            var mensagemSerializada = JsonConvert.SerializeObject(mensagem);
            var corpo = Encoding.UTF8.GetBytes(mensagemSerializada);

            politica.Execute(() =>
            {
                var propriedades = canal.CreateBasicProperties();
                propriedades.DeliveryMode = 2;

                _logger.LogTrace("Publicando o evento para o RabbitMQ: {EventId}", mensagem.EventoId);

                canal.BasicPublish(
                         exchange: BrokerName,
                         routingKey: nomeEvento,
                         mandatory: true,
                         basicProperties: propriedades,
                         body: corpo);
            });
        }

        public void AdicionarInscricao<T, TH>()
             where T : EventoIntegracao
             where TH : IManipuladorEventoIntegracao<T>
        {
            var nomeEvento = _gerenciadorAssinaturas.ObterChaveEvento<T>();
            AssinaturaInterna(nomeEvento);

            _logger.LogInformation("Assinando para o evento {EventName} com {EventHandler}", nomeEvento, typeof(TH).Name);

            _gerenciadorAssinaturas.AdicionarInscricao<T, TH>();
            IniciarConsumoBasico();
        }

        private void AssinaturaInterna(string nomeEvento)
        {
            if (_gerenciadorAssinaturas.ExisteInscricaoParaEvento(nomeEvento))
                return;

            if (!_conexaoPersistente.EstaConectado)
                _ = _conexaoPersistente.TentarConectar();

            using var canal = _conexaoPersistente.CriarModelo();
            canal.QueueBind(queue: _nomeFila, exchange: BrokerName, routingKey: nomeEvento);
        }

        public void RemoverInscricao<T, TH>()
             where T : EventoIntegracao
             where TH : IManipuladorEventoIntegracao<T>
        {
            var nomeEvento = _gerenciadorAssinaturas.ObterChaveEvento<T>();

            _logger.LogInformation("Cancelando a inscrição para o evento {EventName}", nomeEvento);

            _gerenciadorAssinaturas.RemoverInscricao<T, TH>();
        }

        private void IniciarConsumoBasico()
        {
            _logger.LogTrace("Iniciando consumo básico do RabbitMQ");

            if (_canalDeConsumo == null)
            {
                _logger.LogError("IniciarConsumoBasico não pode ser chamado em _consumerChannel == null");
                return;
            }

            var consumidor = new AsyncEventingBasicConsumer(_canalDeConsumo);

            consumidor.Received += Consumidor_Recebido;

            _canalDeConsumo.BasicConsume(queue: _nomeFila, autoAck: false, consumer: consumidor);
        }

        private async Task Consumidor_Recebido(object sender, BasicDeliverEventArgs argumentosEventos)
        {
            var mensagem = Encoding.UTF8.GetString(argumentosEventos.Body.ToArray());

            try
            {
                await ProcessarEvento(argumentosEventos.RoutingKey, mensagem);

                _canalDeConsumo.BasicAck(argumentosEventos.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Processando mensagem \"{Message}\"", mensagem);
            }
        }

        private IModel CriarCanalConsumoBasico()
        {
            if (!_conexaoPersistente.EstaConectado)
                _ = _conexaoPersistente.TentarConectar();

            _logger.LogTrace("Criando canal de consumo do RabbitMQ");

            var canal = _conexaoPersistente.CriarModelo();

            canal.ExchangeDeclare(exchange: BrokerName, type: "direct");

            canal.QueueDeclare(queue: _nomeFila,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

            canal.CallbackException += (sender, ea) =>
            {
                _logger.LogWarning(ea.Exception, "Recriando canal de consumo do RabbitMQ");

                _canalDeConsumo.Dispose();
                _canalDeConsumo = CriarCanalConsumoBasico();
                IniciarConsumoBasico();
            };

            return canal;
        }

        private async Task ProcessarEvento(string nomeEvento, string mensagem)
        {
            _logger.LogTrace("Processando evento do RabbitMQ: {EventName}", nomeEvento);

            if (!_gerenciadorAssinaturas.ExisteInscricaoParaEvento(nomeEvento))
            {
                _logger.LogWarning("Sem assinaturas para o evento do RabbitMQ: {EventName}", nomeEvento);
                return;
            }

            foreach (var assinatura in _gerenciadorAssinaturas.ObterManipuladoresParaEvento(nomeEvento))
            {
                using var escopo = _serviceProvider.CreateScope();

                var manipulador = escopo.ServiceProvider.GetService(assinatura.TipoManipulador);

                if (manipulador == null)
                    continue;

                var tipoEvento = _gerenciadorAssinaturas.ObterTipoEventoPorNome(nomeEvento);
                var eventoIntegracao = JsonConvert.DeserializeObject(mensagem, tipoEvento);
                var tipoConcreto = typeof(IManipuladorEventoIntegracao<>).MakeGenericType(tipoEvento);

                await Task.Yield();
                await (Task)tipoConcreto.GetMethod("ManipularAsync").Invoke(manipulador, new object[] { eventoIntegracao });
            }
        }

        public void Dispose()
        {
            _canalDeConsumo?.Dispose();
            _gerenciadorAssinaturas.Limpar();
            GC.SuppressFinalize(this);
        }
    }
}