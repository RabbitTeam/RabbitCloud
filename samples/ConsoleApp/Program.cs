using Consul;
using Grpc.Core;
using Helloworld;
using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Client;
using Rabbit.Cloud.Client.Abstractions.Extensions;
using Rabbit.Cloud.Client.Breaker.Builder;
using Rabbit.Cloud.Client.Grpc.Builder;
using Rabbit.Cloud.Client.Grpc.Proxy;
using Rabbit.Cloud.Client.LoadBalance;
using Rabbit.Cloud.Client.LoadBalance.Builder;
using Rabbit.Cloud.Client.Proxy;
using Rabbit.Cloud.Discovery.Abstractions;
using Rabbit.Cloud.Discovery.Consul;
using Rabbit.Cloud.Discovery.Consul.Registry;
using Rabbit.Cloud.Discovery.Consul.Utilities;
using Rabbit.Cloud.Grpc.Abstractions;
using Rabbit.Cloud.Grpc.Client;
using Rabbit.Cloud.Grpc.Fluent;
using Rabbit.Cloud.Grpc.Server;
using System;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public interface IServiceBase
    {
        Task<HelloReply> SendAsync(HelloRequest request);
    }

    public class ServiceBase : IServiceBase
    {
        #region Implementation of IServiceBase

        [GrpcMethod]
        public Task<HelloReply> SendAsync(HelloRequest request)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "hello " + request.Name
            });
        }

        #endregion Implementation of IServiceBase
    }

    [GrpcClient("ConsoleApp.TestService")]
    public interface ITestService : IServiceBase
    {
    }

    [GrpcService("ConsoleApp.TestService")]
    public class ServiceImpl : ServiceBase, ITestService
    {
    }

    public class Program
    {
        private const string ConsulUrl = "http://localhost:8500";

        private static async Task StartServer()
        {
            {
                var services = new ServiceCollection()
                    .AddLogging()
                    .AddOptions()
                    .AddConsulRegistry(new ConsulClient(o => o.Address = new Uri(ConsulUrl)))
                    .AddGrpcCore()
                    .AddGrpcServer()
                    .AddGrpcFluent()
                    .AddSingleton<ServiceImpl, ServiceImpl>()
                    .BuildServiceProvider();

                var registryService = services.GetRequiredService<IRegistryService<ConsulRegistration>>();

                await registryService.RegisterAsync(ConsulUtil.Create(new RabbitConsulOptions.DiscoveryOptions
                {
                    HealthCheckInterval = "10s",
                    HostName = "localhost",
                    InstanceId = "localhost_9908",
                    Port = 9908,
                    ServiceName = "ConsoleApp"
                }));

                var serverServiceDefinitionTable = services.GetRequiredService<IServerServiceDefinitionTableProvider>().ServerServiceDefinitionTable;

                {
                    var server = new Server
                    {
                        Ports = { new ServerPort("localhost", 9908, ServerCredentials.Insecure) }
                    };

                    foreach (var definition in serverServiceDefinitionTable)
                    {
                        server.Services.Add(definition);
                    }
                    server.Start();
                }
            }
        }

        private static async Task Main(string[] args)
        {
            await StartServer();

            {
                //client
                var services = new ServiceCollection()
                    .AddLogging()
                    .AddOptions()
                    .AddGrpcCore()
                    .AddGrpcClient()
                    .AddGrpcFluent()
                    .AddConsulDiscovery(c => c.Address = ConsulUrl)
                    .AddLoadBalance()
                    .BuildServiceProvider();

                var app = new RabbitApplicationBuilder(services);
                var invoker = app
                    .Use(async (context, next) =>
                    {
                        context.Request.Url.Host = "ConsoleApp";
                        await next();
                    })
                    .UseBreaker()
                    .UseLoadBalance()
                    .UseGrpc()
                    .Build();

                var rabbitProxyInterceptor = new GrpcProxyInterceptor(invoker);
                var proxyFactory = new ProxyFactory(rabbitProxyInterceptor);
                var service = proxyFactory.CreateInterfaceProxy<ITestService>();
                while (true)
                {
                    var t = await service.SendAsync(new HelloRequest { Name = "test" });
                    Console.WriteLine(t?.Message ?? "null");
                    Console.ReadLine();
                }
            }
        }
    }
}