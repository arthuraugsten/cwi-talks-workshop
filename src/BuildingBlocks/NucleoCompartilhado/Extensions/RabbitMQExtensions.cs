using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NucleoCompartilhado.EventBus;
using RabbitMQ.Client;
using System;

namespace NucleoCompartilhado.Extensions
{
    public static class RabbitMQExtensions
    {
        public static void AdicionarEventBus(this IServiceCollection servicos, IConfiguration configuracao)
        {
            if (configuracao is null) throw new ArgumentNullException(nameof(configuracao));

            servicos.Configure<EventBusConfig>(configuracao.GetSection(nameof(EventBusConfig)));

            servicos.AddSingleton<IGerenciadorAssinaturasEventBus, GerenciadorAssinaturasEmMemoriaEventBus>();
            servicos.AddSingleton<ConexaoPersistentePadraoRabbitMQ, ConexaoPersistentePadraoRabbitMQ>(sp =>
            {
                var eventBusConfig = sp.GetRequiredService<IOptions<EventBusConfig>>().Value;
                var logger = sp.GetRequiredService<ILogger<ConexaoPersistentePadraoRabbitMQ>>();

                var fabricaConexoes = new ConnectionFactory
                {
                    HostName = eventBusConfig.Host,
                    Port = eventBusConfig.Porta,
                    UserName = eventBusConfig.Usuario,
                    Password = eventBusConfig.Senha,
                    DispatchConsumersAsync = true
                };

                return new ConexaoPersistentePadraoRabbitMQ(fabricaConexoes, logger, eventBusConfig.QuantidadeTentativas);
            });

            servicos.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
            {
                var conexaoPersistentePadraoRabbitMQ = sp.GetRequiredService<ConexaoPersistentePadraoRabbitMQ>();
                var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                var gerenciadorAssinaturasEventBus = sp.GetRequiredService<IGerenciadorAssinaturasEventBus>();
                var eventBusConfig = sp.GetRequiredService<IOptions<EventBusConfig>>().Value;

                return new EventBusRabbitMQ(conexaoPersistentePadraoRabbitMQ,
                         logger,
                         sp,
                         gerenciadorAssinaturasEventBus,
                         eventBusConfig.NomeInscricao,
                         eventBusConfig.QuantidadeTentativas);
            });
        }
    }
}