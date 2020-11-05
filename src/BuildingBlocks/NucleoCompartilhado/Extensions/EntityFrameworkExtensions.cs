using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Linq;

namespace NucleoCompartilhado.Extensions
{
    public static class EntityFrameworkExtensions
    {
        public static IServiceProvider MigrarContexto<TContext>(this IServiceProvider provedorServicos) where TContext : DbContext
          => MigrarContexto<TContext>(provedorServicos, (_, __) => { });

        public static IServiceProvider MigrarContexto<TContext>(this IServiceProvider provedorServicos, Action<TContext, IServiceProvider> seeder) where TContext : DbContext
        {
            var logger = provedorServicos.GetRequiredService<ILogger<TContext>>();
            var contexto = provedorServicos.GetService<TContext>();

            try
            {
                logger.LogInformation("Migrando banco de dados {DbContextName}", typeof(TContext).Name);

                var tentativas = 10;
                var politica = Policy.Handle<Exception>()
                     .WaitAndRetry(
                          retryCount: tentativas,
                          sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                          onRetry: (exception, timeSpan, ret, ctx) =>
                          {
                              logger.LogWarning(exception, "[{prefix}] Exceção {ExceptionType} com mensagem {Message} detectada na tentativa {ret} de {retries}", nameof(TContext), exception.GetType().Name, exception.Message, ret, tentativas);
                          });

                politica.Execute(() => InvokeSeeder(seeder, contexto, provedorServicos));

                logger.LogInformation("Banco de dados {DbContextName} migrado", typeof(TContext).Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Um erro aconteceu ao migrar o banco de dados com contexto {DbContextName}", typeof(TContext).Name);
            }

            return provedorServicos;
        }

        private static void InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder, TContext context, IServiceProvider servicos)
             where TContext : DbContext
        {
            context.Database.Migrate();
            seeder(context, servicos);
        }

        public static void ConfigurarStrings(this ModelBuilder modelBuilder)
        {
            var propriedades = modelBuilder.Model.GetEntityTypes()
                 .SelectMany(t => t.GetProperties())
                 .Where(p => p.ClrType == typeof(string));

            foreach (var property in propriedades)
            {
                if (property.GetMaxLength() == null)
                    property.SetMaxLength(256);

                property.SetIsUnicode(false);
            }
        }
    }
}
