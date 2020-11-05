using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NucleoCompartilhado.Extensions;
using System;

namespace Tarefas.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static void AdicionarDependências(this IServiceCollection serviços, IConfiguration configurações)
        {
            if (serviços is null) throw new ArgumentNullException(nameof(serviços));
            if (configurações is null) throw new ArgumentNullException(nameof(serviços));

            serviços.AdicionarSwagger(configurações);
        }
    }
}