using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Server.Grpc.Builder;
using Rabbit.Cloud.Server.Monitor.Builder;
using Rabbit.Extensions.Boot;
using Rabbit.Extensions.Configuration;
using Samples.Service;
using System.Threading.Tasks;

namespace Samples.Server
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var hostBuilder = await RabbitBoot.BuildHostBuilderAsync(builder =>
            {
                builder
                    .ConfigureHostConfiguration(b => b.AddJsonFile("appsettings.json"))
                    .ConfigureServices(s =>
                    {
                        s.AddSingleton<TestService>();
                    })
                    .ConfigureContainer<IServiceCollection>((ctx, services) =>
                    {
                        var app = new RabbitApplicationBuilder(services.BuildServiceProvider())
                            .UseAllMonitor()
                            .UseServerGrpc()
                            .Build();

                        services.AddSingleton(app);
                    });
            });

            hostBuilder
                .ConfigureContainer<IServiceCollection>((ctx, s) =>
                {
                    ctx.Configuration.EnableTemplateSupport();
                })
                .UseConsoleLifetime();

            await hostBuilder.RunConsoleAsync();
        }
    }
}