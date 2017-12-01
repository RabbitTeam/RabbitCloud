using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Grpc.Builder;
using Rabbit.Cloud.Client.Grpc.Proxy;
using Rabbit.Cloud.Client.LoadBalance;
using Rabbit.Cloud.Client.LoadBalance.Builder;
using Rabbit.Cloud.Client.Proxy;
using Rabbit.Cloud.Discovery.Configuration;
using Rabbit.Cloud.Grpc.Abstractions;
using Rabbit.Cloud.Grpc.Client;
using Rabbit.Cloud.Grpc.Fluent;
using Rabbit.Cloud.Grpc.Server;
using Rabbit.Cloud.Serialization.Json;
using Rabbit.Cloud.Serialization.MessagePack;
using Rabbit.Cloud.Serialization.Protobuf;
using Rabbit.Cloud.Server.Grpc;
using Rabbit.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class ApplicationInfo
    {
        public string ServiceName { get; set; }
        public string HostName { get; set; }
        public int HostPort { get; set; }
    }

    public class Program
    {
        private static ApplicationInfo ApplicationInfo { get; set; }

        private static async Task Main(string[] args)
        {
            ApplicationInfo = BuildConfiguration(args).Get<ApplicationInfo>();

            // start server
            StartServer();

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

        private static void StartServer()
        {
            {
                IServiceProvider services = new ServiceCollection()
                    .AddLogging()
                    .AddOptions()
                    .AddSingleton<TestService, TestService>()
                    .AddGrpcCore()
                    .AddGrpcServer()
                    .AddGrpcFluent(options =>
                    {
                        options
                            .Serializers
                            .AddProtobufSerializer()
                            .AddMessagePackSerializer()
                            .AddJsonSerializer();
                    })
                    .AddServerGrpc()
                    .BuildServiceProvider();

                var serverServiceDefinitionTable = services.GetRequiredService<IServerServiceDefinitionTableProvider>().ServerServiceDefinitionTable;

                {
                    var server = new Server
                    {
                        Ports = { new ServerPort(ApplicationInfo.HostName, ApplicationInfo.HostPort, ServerCredentials.Insecure) }
                    };

                    foreach (var definition in serverServiceDefinitionTable)
                    {
                        server.Services.Add(definition);
                    }
                    server.Start();
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
                .AddLogging(options =>
                {
                    options.AddConsole(s =>
                    {
                        s.IncludeScopes = true;
                    });
                    options.SetMinimumLevel(LogLevel.Information);
                })
                .AddOptions()
                .AddGrpcCore()
                .AddGrpcClient()
                .AddGrpcFluent(options =>
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

        private static IConfiguration BuildConfiguration(string[] args)
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddCommandLine(args)
                .Build()
                .EnableTemplateSupport();
        }
    }
}