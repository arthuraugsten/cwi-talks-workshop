using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;

namespace ApiGateway
{
    public sealed class Startup
    {
        private const string NomePoliticaCors = "AllowAll";

        public Startup(IConfiguration configurações)
            => Configuração = configurações;

        public IConfiguration Configuração { get; }

        public void ConfigureServices(IServiceCollection serviços)
        {
            serviços.AddCors(options =>
               options.AddPolicy(NomePoliticaCors, p
                   => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
               )
            );

            serviços.AddControllers();
            serviços.AddOcelot().AddConsul();
        }

        public void Configure(IApplicationBuilder aplicativo, IWebHostEnvironment ambiente)
        {
            if (ambiente.IsDevelopment())
                aplicativo.UseDeveloperExceptionPage();

            aplicativo.UseOcelot().Wait();

            aplicativo.UseCors(NomePoliticaCors);
            aplicativo.UseRouting();
        }
    }
}
