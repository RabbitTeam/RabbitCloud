using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Client.Proxy;
using Rabbit.Cloud.Grpc.ApplicationModels;
using Rabbit.Cloud.Hosting;
using Rabbit.Extensions.Boot;
using Samples.Service;
using System;
using System.Threading.Tasks;

namespace Samples.Client
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            var hostBuilder = await RabbitBoot.BuildHostBuilderAsync(builder =>
            {
                builder
                    .ConfigureHostConfiguration(b => b.AddJsonFile("appsettings.json"))
                    .UseRabbitApplicationConfigure();
            });

            var host = hostBuilder.BuidRabbitHost();
            await host.StartAsync();

            var proxyFactory = host.Services.GetRequiredService<IProxyFactory>();

            foreach (var serviceModel in host.Services.GetRequiredService<ApplicationModelHolder>().GetApplicationModel().Services)
            {
                services.AddSingleton(serviceModel.Type, proxyFactory.CreateInterfaceProxy(serviceModel.Type));
            }

            {
                var service = services.BuildServiceProvider().GetRequiredService<ITestService>();
                var name = "test";
                while (true)
                {
                    try
                    {
                        var request = new Request
                        {
                            Name = name
                        };
                        var response = await service.SendAsync(request);
                        Console.WriteLine($"SendAsync:{response.Message}");
                        response = await service.Send2Async(name, 10);
                        Console.WriteLine($"Send2Async:{response.Message}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    finally
                    {
                        name = Console.ReadLine();
                    }
                }
            }
        }
    }
}