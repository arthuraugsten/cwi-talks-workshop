using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NucleoCompartilhado;
using Tarefas.Dados.Contextos;
using Tarefas.Extensions;

namespace Tarefas
{
    public sealed class Startup : BaseStartup
    {
        public Startup(IConfiguration configurações)
            : base(configurações) { }

        public void ConfigureServices(IServiceCollection serviços)
            => ConfigurarServiços<TarefasContexto>(serviços, serviços.AdicionarDependências);

        public void Configure(IApplicationBuilder aplicativo, IWebHostEnvironment ambiente)
            => Configurar<TarefasContexto>(aplicativo, ambiente, aplicativo.UsarEventosIntegrações);
    }
}