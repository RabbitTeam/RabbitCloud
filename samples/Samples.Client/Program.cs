using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rabbit.Cloud;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Client.Grpc.Proxy;
using Rabbit.Cloud.Client.Proxy;
using Rabbit.Cloud.Discovery.Configuration;
using Rabbit.Cloud.Grpc;
using Rabbit.Cloud.Grpc.ApplicationModels;
using Rabbit.Extensions.Boot;
using Samples.Service;
using System;
using System.Threading.Tasks;
using Rabbit.Cloud.Client.LoadBalance.Builder;

namespace Samples.Client
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var hostBuilder = await RabbitBoot.BuildHostBuilderAsync(builder =>
            {
                builder.ConfigureHostConfiguration(b =>
                {
                    b.AddJsonFile("appsettings.json");
                });
                builder.ConfigureServices(services =>
                    {
                        var instancesConfiguration = new ConfigurationBuilder()
                            .AddJsonFile("instances.json", false, true)
                            .Build();
                        services
                            .AddLogging()
                            .AddOptions()
                            .AddGrpcClient(o => { })
                            .AddConfigurationDiscovery(instancesConfiguration);
                    })
                    .UseRabbitApplicationConfigure();
                builder.ConfigureRabbitApplication(app =>
                {
                    app.UseLoadBalance();
                });
            });

            hostBuilder.ConfigureRabbitApplication((ctx, services, applicationServices, appBuilder) =>
            {
                var app = appBuilder.Build();

                var rabbitProxyInterceptor = new GrpcProxyInterceptor(app, applicationServices.GetRequiredService<IOptions<RabbitCloudOptions>>());

                var proxyFactory = new ProxyFactory(rabbitProxyInterceptor);

                foreach (var serviceModel in applicationServices.GetRequiredService<ApplicationModelHolder>().GetApplicationModel().Services)
                {
                    services.AddSingleton(serviceModel.Type, proxyFactory.CreateInterfaceProxy(serviceModel.Type));
                }
            });

            var host = hostBuilder.Build();

            {
                var service = host.Services.GetRequiredService<ITestService>();
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