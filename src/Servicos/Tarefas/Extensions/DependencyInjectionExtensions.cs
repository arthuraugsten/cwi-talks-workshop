using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NucleoCompartilhado.EventBus;
using NucleoCompartilhado.Extensions;
using System;
using Tarefas.Eventos;

namespace Tarefas.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static void AdicionarDependências(this IServiceCollection serviços, IConfiguration configurações)
        {
            if (serviços is null) throw new ArgumentNullException(nameof(serviços));
            if (configurações is null) throw new ArgumentNullException(nameof(serviços));

            serviços.AdicionarSwagger(configurações);
            serviços.AdicionarEventBus(configurações);
            serviços.AdicionarEventosIntegração();
        }

        private static void AdicionarEventosIntegração(this IServiceCollection serviços)
        {
            serviços.AddScoped<IManipuladorEventoIntegracao<UsuarioCriadoEventoIntegracao>, UsuarioCriadoManipuladorEventoIntegracao>();
            serviços.AddScoped<IManipuladorEventoIntegracao<UsuarioEditadoEventoIntegracao>, UsuarioEditadoManipuladorEventoIntegracao>();
        }

        public static void UsarEventosIntegrações(this IApplicationBuilder aplicativo)
        {
            if (aplicativo is null) throw new ArgumentNullException(nameof(aplicativo));

            var eventBus = aplicativo.ApplicationServices.GetRequiredService<IEventBus>();

            eventBus.AdicionarInscricao<UsuarioCriadoEventoIntegracao, IManipuladorEventoIntegracao<UsuarioCriadoEventoIntegracao>>();
            eventBus.AdicionarInscricao<UsuarioEditadoEventoIntegracao, IManipuladorEventoIntegracao<UsuarioEditadoEventoIntegracao>>();
        }
    }
}