using Grpc.Core;
using Helloworld;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Application.Abstractions.Extensions;
using Rabbit.Cloud.Client.Breaker;
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
using Rabbit.Cloud.Server.Grpc;
using Rabbit.Cloud.Server.Grpc.Builder;
using Rabbit.Extensions.Configuration;
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
            if (request.Name == "error")
            {
                throw new Exception("error");
            }
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
        private const string ConsulUrl = "http://192.168.100.150:8500";

        private static async Task StartServer()
        {
            {
                IServiceProvider services = new ServiceCollection()
                    .AddLogging()
                    .AddOptions()
                    .Configure<RabbitConsulOptions>(_configuration.GetSection("RabbitCloud:Consul"))
                    .AddConsulRegistry()
                    .AddGrpcCore()
                    .AddGrpcServer()
                    .AddGrpcFluent()
                    .AddSingleton<ServiceImpl, ServiceImpl>()
                    .AddServerGrpc(options =>
                    {
                        var serverServices = new ServiceCollection()
                            .AddOptions()
                            .BuildServiceProvider();
                        var serverApp = new RabbitApplicationBuilder(serverServices)
                            .UseServerGrpc()
                            .Build();

                        options.Invoker = serverApp;
                    })
                    .BuildServiceProvider();

                var registryService = services.GetRequiredService<IRegistryService<ConsulRegistration>>();

                var rabbitConsulOptions = services.GetRequiredService<IOptions<RabbitConsulOptions>>().Value;

                await registryService.RegisterAsync(ConsulUtil.Create(rabbitConsulOptions.Discovery));

                var serverServiceDefinitionTable = services.GetRequiredService<IServerServiceDefinitionTableProvider>().ServerServiceDefinitionTable;

                {
                    var server = new Server
                    {
                        Ports = { new ServerPort(rabbitConsulOptions.Discovery.HostName, rabbitConsulOptions.Discovery.Port, ServerCredentials.Insecure) }
                    };

                    foreach (var definition in serverServiceDefinitionTable)
                    {
                        server.Services.Add(definition);
                    }
                    server.Start();
                }
            }
        }

        private static IConfiguration _configuration;

        private static async Task Main(string[] args)
        {
            _configuration = BuildConfiguration(args);
            /*            await StartServer();
                        Console.WriteLine("press key exit...");
                        Console.ReadLine();
                        return;*/
            {
                //client
                var services = new ServiceCollection()
                    .AddLogging()
                    .AddOptions()
                    .AddGrpcCore()
                    .AddGrpcClient()
                    .AddGrpcFluent()
                    .AddConsulDiscovery(_configuration)
                    .AddBreaker()
                    .AddSingleton<FailureTable, FailureTable>()
                    .Configure<BackoffOptions>(options =>
                    {
                        options.IsFailure = ctx =>
                        {
                            if (!(ctx.Exception is RpcException rpcException))
                                return false;

                            switch (rpcException.Status.StatusCode)
                            {
                                case StatusCode.DeadlineExceeded:
                                case StatusCode.ResourceExhausted:
                                case StatusCode.Unavailable:
                                    return true;
                            }

                            return false;
                        };
                    })
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
                    .UseMiddleware<BackoffMiddleware>()
                    .UseLoadBalance()
                    .UseGrpc()
                    .Build();

                var rabbitProxyInterceptor = new GrpcProxyInterceptor(invoker);
                var proxyFactory = new ProxyFactory(rabbitProxyInterceptor);
                var service = proxyFactory.CreateInterfaceProxy<ITestService>();

                var name = "test";
                while (true)
                {
                    try
                    {
                        var t = await service.SendAsync(new HelloRequest { Name = name ?? "test" });
                        Console.WriteLine(t?.Message ?? "null");
                        name = Console.ReadLine();
                    }
                    catch (Exception e)
                    {
                        //                        Console.WriteLine(e.Message);
                    }
                }
            }
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