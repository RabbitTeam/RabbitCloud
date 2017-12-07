using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Grpc.Builder;
using Rabbit.Cloud.Client.Grpc.Proxy;
using Rabbit.Cloud.Client.LoadBalance;
using Rabbit.Cloud.Client.LoadBalance.Builder;
using Rabbit.Cloud.Client.Proxy;
using Rabbit.Cloud.Discovery.Configuration;
using Rabbit.Cloud.Grpc;
using Rabbit.Cloud.Serialization.Json;
using Rabbit.Cloud.Serialization.MessagePack;
using Rabbit.Cloud.Serialization.Protobuf;
using Samples.Service;
using System;
using System.Threading.Tasks;

namespace Samples.Client
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // init client
            var proxyFactory = BuildClientProxyFactory();

            {
                var service = proxyFactory.CreateInterfaceProxy<ITestService>();
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
                    finally
                    {
                        name = Console.ReadLine();
                    }
                }
            }
        }

        private static RabbitRequestDelegate GetClientApplication(IServiceProvider services)
        {
            var app = new RabbitApplicationBuilder(services);
            return app
                .UseLoadBalance()
                .UseGrpc()
                .Build();
        }

        private static IServiceProvider BuildClientServices()
        {
            var instancesConfiguration = new ConfigurationBuilder()
                .AddJsonFile("instances.json", false, true)
                .Build();

            //client
            return new ServiceCollection()
                .AddLogging()
                .AddOptions()
                .AddGrpcClient(options =>
                {
                    options
                        .Serializers
                        .AddProtobufSerializer()
                        .AddMessagePackSerializer()
                        .AddJsonSerializer();
                })
                .AddConfigurationDiscovery(instancesConfiguration)
                .AddLoadBalance()
                .BuildServiceProvider();
        }

        private static IProxyFactory BuildClientProxyFactory()
        {
            var services = BuildClientServices();
            var application = GetClientApplication(services);
            var rabbitProxyInterceptor = new GrpcProxyInterceptor(application, services.GetRequiredService<IOptions<GrpcOptions>>());
            var proxyFactory = new ProxyFactory(rabbitProxyInterceptor);

            return proxyFactory;
        }
    }
}