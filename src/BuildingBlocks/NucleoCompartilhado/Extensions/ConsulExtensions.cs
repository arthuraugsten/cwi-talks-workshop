using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NucleoCompartilhado.Config;
using Polly;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;

namespace NucleoCompartilhado.Extensions
{
    public static class ConsulExtensions
    {
        public static IServiceCollection AdicionarConsul(this IServiceCollection serviços, IConfiguration configurações)
        {
            var (urlConsul, aclToken) = configurações.GetSection(nameof(ConsulConfig)).Get<ConsulConfig>();

            serviços.AddSingleton<IConsulClient, ConsulClient>(p =>
            {
                var cliente = new ConsulClient(consulConfig =>
                {
                    consulConfig.Address = new Uri(urlConsul);
                    if (!string.IsNullOrEmpty(aclToken))
                        consulConfig.Token = aclToken;
                });

                return cliente;
            });

            return serviços;
        }

        public static IServiceCollection AdicionarConfiguracaoConsul<T>(this IServiceCollection servicos)
            where T : class, new()
        {
            servicos.TryAddSingleton<T>(provedor =>
            {
                var clienteConsul = provedor.GetRequiredService<IConsulClient>();

                var resultadoExecucao = Polly.Policy
                    .Handle<HttpRequestException>()
                    .OrResult<byte[]>(t => t == null || !t.Any())
                    .WaitAndRetry(4, tentativa => TimeSpan.FromSeconds(tentativa * 2))
                    .ExecuteAndCapture(() => clienteConsul.KV.Get(typeof(T).Name).Result.Response.Value);

                var respostaConfiguracoes = Encoding.UTF8.GetString(resultadoExecucao.Result);

                return JsonConvert.DeserializeObject<T>(respostaConfiguracoes);

            });

            return servicos;
        }

        public static IApplicationBuilder UsarConsul<T>(this IApplicationBuilder aplicativo, IConfiguration configurações, IWebHostEnvironment ambiente)
        {
            var logger = aplicativo.ApplicationServices.GetRequiredService<ILogger<T>>();
            var nomeServico = configurações.GetValue<string>($"{nameof(ConsulConfig)}:{nameof(ConsulConfig.NomeServico)}");

            (string host, int porta) enderecoAplicacao;
            if (!ambiente.IsDevelopment())
            {
                const int portaPadrao = 80;
                var ip = Dns.GetHostEntry(Dns.GetHostName())
                    .AddressList
                    .First(x => x.AddressFamily == AddressFamily.InterNetwork)
                    .ToString();

                enderecoAplicacao.host = ip;
                enderecoAplicacao.porta = portaPadrao;
            }
            else
            {
                var serverAddressesFeature = aplicativo.ServerFeatures.Get<IServerAddressesFeature>();
                var endereco = serverAddressesFeature?.Addresses?.FirstOrDefault();

                if (string.IsNullOrEmpty(endereco)) return aplicativo;

                var uri = new Uri(endereco);

                enderecoAplicacao.host = uri.Host;
                enderecoAplicacao.porta = uri.Port;
            }

            var consulClient = aplicativo.ApplicationServices.GetRequiredService<IConsulClient>();
            var lifetime = aplicativo.ApplicationServices
                .GetRequiredService<IHostApplicationLifetime>();

            var idUnico =
                Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..^2];
            // Isso é só pra deixar o id mais curto

            var registration = new AgentServiceRegistration
            {
                ID = $"{nomeServico}-{idUnico}",
                Name = nomeServico,
                Address = enderecoAplicacao.host,
                Port = enderecoAplicacao.porta
            };

            if (!ambiente.IsDevelopment())
            {
                registration.Checks = new[] {
                    new AgentServiceCheck
                    {
                        DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(2),
                        Interval = TimeSpan.FromSeconds(30),
                        HTTP = $"http://{enderecoAplicacao.host}:{enderecoAplicacao.porta}/api/saude"
                    }
                };
            }

            logger?.LogInformation($"Registrando aplicação no Consul com id {registration.ID}");
            consulClient.Agent.ServiceRegister(registration).Wait();

            lifetime.ApplicationStopping.Register(() =>
            {
                logger?.LogInformation("Removendo registro no Consul");

                var service = consulClient.Catalog.Service(registration.Name)
                    .Result
                    .Response
                    .FirstOrDefault(x => x.ServiceID == registration.ID);

                if (service is object)
                    consulClient.Catalog.Deregister(new CatalogDeregistration { Node = service.Node, ServiceID = service.ServiceID });
            });

            return aplicativo;
        }

        public static IApplicationBuilder CompartilharConfiguracaoConsul<T>(this IApplicationBuilder app)
            where T : class, new()
        {
            var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
            var valor = app.ApplicationServices.GetRequiredService<IOptions<T>>().Value;

            consulClient.KV.Put(new KVPair(typeof(T).Name)
            {
                Value = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(valor))
            }).Wait();

            return app;
        }
    }
}
