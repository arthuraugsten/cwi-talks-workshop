using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace NucleoCompartilhado.Extensions
{
    public static class VerificacaoSaudeExtensions
    {
        public const string SituacaoSaudavel = "Saudável";

        private static Task RespostaPadrao(HttpContext contexto, HealthReport resultado)
        {
            contexto.Response.ContentType = "application/json";

            var json = new JObject(
                new JProperty("situacao", SituacaoSaudavel),
                new JProperty("tempoVerificacao", resultado.TotalDuration)
            );

            return contexto.Response.WriteAsync(json.ToString(Formatting.Indented));
        }

        public static void AdicionarVerificacaoSaude(this IServiceCollection servicos)
            => servicos.AddHealthChecks();

        public static void UsarRotasEVerificacaoSaude(this IApplicationBuilder app)
            => app.UseEndpoints(configuracao =>
            {
                configuracao.MapControllers();
                configuracao.MapHealthChecks("/api/saude", new HealthCheckOptions()
                {
                    AllowCachingResponses = false,
                    ResponseWriter = RespostaPadrao
                });
            });
    }
}