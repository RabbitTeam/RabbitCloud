using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabbit.Cloud.Abstractions.Serialization;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Application.Abstractions.Extensions;
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
using Rabbit.Cloud.Serialization.Json;
using Rabbit.Cloud.Serialization.MessagePack;
using Rabbit.Cloud.Serialization.Protobuf;
using Rabbit.Cloud.Server.Grpc;
using Rabbit.Cloud.Server.Grpc.Builder;
using Rabbit.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class Request
    {
        public string Name { get; set; }
    }

    public class Response
    {
        public string Message { get; set; }
    }

    [GrpcClient("ConsoleApp.TestService")]
    public interface ITestService
    {
        Task<Response> SendAsync(Request request);
    }

    [GrpcService("ConsoleApp.TestService")]
    public class TestService : ITestService
    {
        #region Implementation of IServiceBase

        public Task<Response> SendAsync(Request request)
        {
            return Task.FromResult(new Response
            {
                Message = "hello " + request.Name
            });
        }

        #endregion Implementation of IServiceBase
    }

    public class Program
    {
        private static async Task StartServer()
        {
            {
                var rabbitConsulOptions = _configuration.GetSection("RabbitCloud:Consul").Get<RabbitConsulOptions>();
                IServiceProvider services = new ServiceCollection()
                    .AddLogging()
                    .AddOptions()
                    .Configure<RabbitConsulOptions>(_configuration.GetSection("RabbitCloud:Consul"))
                    .AddConsulRegistry()
                    .AddGrpcCore()
                    .AddGrpcServer()
                    .AddGrpcFluent()
                    .AddSingleton<TestService, TestService>()
                    .AddJsonSerializer()
                    .AddProtobufSerializer()
                    .AddMessagePackSerializer()
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
            await StartServer();

            {
                //client
                var services = new ServiceCollection()
                    .AddLogging(options =>
                    {
                        options.AddConsole(s =>
                        {
                            s.IncludeScopes = true;
                        });
                        options.SetMinimumLevel(LogLevel.Information);
                    })
                    .AddJsonSerializer()
                    .AddProtobufSerializer()
                    .AddMessagePackSerializer()
                    .AddOptions()
                    .AddGrpcCore()
                    .AddGrpcClient()
                    .AddGrpcFluent()
                    .AddConsulDiscovery(_configuration)
                    .AddLoadBalance()
                    .BuildServiceProvider();

                var app = new RabbitApplicationBuilder(services);
                var invoker = app
                    .Use(async (context, next) =>
                    {
                        context.Request.Url.Host = "ConsoleApp";
                        await next();
                    })
                    .UseLoadBalance()
                    .UseGrpc()
                    .Build();

                var rabbitProxyInterceptor = new GrpcProxyInterceptor(invoker, services.GetRequiredService<IEnumerable<ISerializer>>());
                var proxyFactory = new ProxyFactory(rabbitProxyInterceptor);
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
                        Console.WriteLine(response.Message);
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