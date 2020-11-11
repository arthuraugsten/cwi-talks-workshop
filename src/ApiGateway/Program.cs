using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ApiGateway
{
    public sealed class Program
    {
        public static void Main(string[] args)
            => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webHostBuilder =>
                    webHostBuilder
                        .ConfigureAppConfiguration((webHostBuilderCtx, configBuilder) =>
                            configBuilder
                                .AddJsonFile("ocelot.json")
                                .AddJsonFile($"ocelot.{webHostBuilderCtx.HostingEnvironment.EnvironmentName}.json")
                        )
                        .UseStartup<Startup>());
    }
}
