using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using NucleoCompartilhado.Config;
using NucleoCompartilhado.Filtros;
using System;
using System.Collections.Generic;

namespace NucleoCompartilhado.Extensions
{
    public static class SwaggerExtensions
    {
        public static void AdicionarSwagger(this IServiceCollection serviços, IConfiguration configurações)
        {
            if (serviços is null) throw new ArgumentNullException(nameof(serviços));
            if (configurações is null) throw new ArgumentNullException(nameof(configurações));

            serviços.Configure<SwaggerConfig>(configurações.GetSection(nameof(SwaggerConfig)));

            var provedorServicos = serviços.BuildServiceProvider();

            var configuracaoSwagger = provedorServicos.GetRequiredService<IOptions<SwaggerConfig>>().Value;

            serviços.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(configuracaoSwagger.Versao, new OpenApiInfo
                {
                    Title = configuracaoSwagger.Titulo,
                    Version = configuracaoSwagger.Versao
                });

                options.DescribeAllParametersInCamelCase();
                options.DocumentFilter<FiltroDeVerificacaoSaude>();
                options.CustomSchemaIds(c => c.FullName);
            });
            serviços.AddSwaggerGenNewtonsoftSupport();
        }

        public static void UsarSwagger(this IApplicationBuilder aplicativo, IConfiguration configurações)
        {
            var configuracaoSwagger = aplicativo.ApplicationServices.GetRequiredService<IOptions<SwaggerConfig>>().Value;
            var urlApiGateway = configurações.GetValue<string>($"{nameof(ApiGatewayConfig)}{nameof(ApiGatewayConfig.Url)}");
            var nomeServiço = configurações.GetValue<string>($"{nameof(ConsulConfig)}:{nameof(ConsulConfig.NomeServico)}");

            var servidores = new List<OpenApiServer>(1)
            {
                new OpenApiServer { Url = $"{urlApiGateway}/{nomeServiço}" }
            };

            aplicativo.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((docOpenApi, reqHttp) =>
                {
                    servidores.Add(new OpenApiServer { Url = $"{reqHttp.Scheme}://{reqHttp.Host}" });
                    docOpenApi.Servers = servidores;
                });
            });
            aplicativo.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("v1/swagger.json", configuracaoSwagger.Titulo);
                options.OAuthAppName(configuracaoSwagger.NomeAplicativo);
                options.OAuthClientSecret(string.Empty);
                options.OAuthRealm(string.Empty);
                options.OAuthUsePkce();
            });
        }
    }
}