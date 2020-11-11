using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NucleoCompartilhado.Extensions;
using System;

namespace NucleoCompartilhado
{
    public abstract class BaseStartup
    {
        private const string NomePoliticaCors = "AllowAll";

        public BaseStartup(IConfiguration configuracao)
                => Configuração = configuracao;

        public IConfiguration Configuração { get; }

        public void ConfigurarServiços<TContexto>(IServiceCollection serviços, Action<IConfiguration> injeçãoDependências = default)
            where TContexto : DbContext
        {
            injeçãoDependências?.Invoke(Configuração);
            serviços.AdicionarConsul(Configuração);
            serviços.AdicionarVerificacaoSaude();

            serviços.AddDbContext<TContexto>(options =>
                    options.UseSqlServer(
                        Configuração.GetConnectionString("Default"),
                        sqlActions => sqlActions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(1), default))
            );

            serviços.AddCors(options =>
              options.AddPolicy(NomePoliticaCors, p
                  => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
              )
            );

            serviços.AddControllers().AddNewtonsoftJson();
        }

        public void Configurar<TContexto>(IApplicationBuilder aplicativo, IWebHostEnvironment ambiente, Action eventBus = default)
            where TContexto : DbContext
        {
            if (ambiente.IsDevelopment())
                aplicativo.UseDeveloperExceptionPage();

            aplicativo.UsarSwagger(Configuração);
            aplicativo.UseCors(NomePoliticaCors);

            aplicativo.UseRouting();

            aplicativo.UseRewriter(new RewriteOptions().AddRedirect("^$", "swagger"));
            aplicativo.UsarRotasEVerificacaoSaude();

            aplicativo.UseEndpoints(endpoints => endpoints.MapControllers());

            aplicativo.UsarConsul<BaseStartup>(Configuração, ambiente);

            using var escopo = aplicativo.ApplicationServices.CreateScope();
            escopo.ServiceProvider.MigrarContexto<TContexto>();

            eventBus?.Invoke();
        }
    }
}