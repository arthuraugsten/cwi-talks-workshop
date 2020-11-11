using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NucleoCompartilhado;
using Usuarios.Dados.Contextos;
using Usuarios.Extensions;

namespace Usuarios
{
    public sealed class Startup : BaseStartup
    {
        public Startup(IConfiguration configurações)
            : base(configurações) { }

        public void ConfigureServices(IServiceCollection serviços)
            => ConfigurarServiços<UsuáriosContexto>(serviços, serviços.AdicionarDependências);

        public void Configure(IApplicationBuilder aplicativo, IWebHostEnvironment ambiente)
            => Configurar<UsuáriosContexto>(aplicativo, ambiente);
    }
}