using Consul;
using Grpc.Core;
using Helloworld;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Rabbit.Cloud.Client;
using Rabbit.Cloud.Client.Abstractions.Extensions;
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
using Rabbit.Cloud.Grpc.Abstractions.Method;
using Rabbit.Cloud.Grpc.Client;
using Rabbit.Cloud.Grpc.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class Program
    {
        public class TestService
        {
            /// <summary>
            /// Sends a greeting
            /// </summary>
            /// <param name="request">The request received from the client.</param>
            /// <param name="context">The context of the server-side call handler being invoked.</param>
            /// <returns>The response to send back to the client (wrapped by a task).</returns>
            [GrpcService(MethodName = "test")]
            public Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
            {
                Console.WriteLine(context.Host);
                return Task.FromResult(new HelloReply { Message = "hello " + request.Name });
            }
        }

        [GrpcService(ServiceName = "consoleapp.TestService")]
        public interface ITestService
        {
            [GrpcService(MethodName = "test")]
            AsyncUnaryCall<HelloReply> HelloAsync(HelloRequest request);

            [GrpcService(MethodName = "test")]
            void Hello(HelloRequest request);

            [GrpcService(MethodName = "test")]
            Task HelloAsync2(HelloRequest request);

            [GrpcService(MethodName = "test")]
            Task<HelloReply> HelloAsync3(HelloRequest request);
        }

        private const string ConsulUrl = "http://192.168.1.150:8500";

        private static async Task StartServer(IMethodCollection methodCollection)
        {
            {
                var services = new ServiceCollection()
                    .AddLogging()
                    .AddOptions()
                    .AddConsulRegistry(new ConsulClient(o => o.Address = new Uri(ConsulUrl)))
                    .AddSingleton(methodCollection)
                    .AddSingleton(
                        new DefaultServerServiceDefinitionProviderOptions
                        {
                            Factory = t => new TestService(),
                            Types = new[] { typeof(TestService) }
                        })
                    .AddSingleton<IServerServiceDefinitionProvider, DefaultServerServiceDefinitionProvider>()
                    .BuildServiceProvider();

                var registryService = services.GetRequiredService<IRegistryService<ConsulRegistration>>();

                await registryService.RegisterAsync(ConsulUtil.Create(new RabbitConsulOptions.DiscoveryOptions
                {
                    HealthCheckInterval = "10s",
                    HostName = "localhost",
                    InstanceId = "localhost_9907",
                    Port = 9907,
                    ServiceName = "ConsoleApp"
                }));
                await registryService.RegisterAsync(ConsulUtil.Create(new RabbitConsulOptions.DiscoveryOptions
                {
                    HealthCheckInterval = "10s",
                    HostName = "localhost",
                    InstanceId = "localhost_9908",
                    Port = 9908,
                    ServiceName = "ConsoleApp"
                }));

                var serverServiceDefinitionProvider = services.GetRequiredService<IServerServiceDefinitionProvider>();

                var definitions = serverServiceDefinitionProvider.GetDefinitions().ToArray();
                {
                    var server = new Server
                    {
                        Ports = { new ServerPort("localhost", 9907, ServerCredentials.Insecure) }
                    };
                    foreach (var definition in definitions)
                    {
                        server.Services.Add(definition);
                    }
                    server.Start();
                }
                {
                    var server = new Server
                    {
                        Ports = { new ServerPort("localhost", 9908, ServerCredentials.Insecure) }
                    };
                    foreach (var definition in definitions)
                    {
                        server.Services.Add(definition);
                    }
                    server.Start();
                }
            }
        }

        private static IMethodCollection GetMethodCollection()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .AddOptions()
                .AddGrpcCore(typeof(TestService))
                .BuildServiceProvider();

            var methodCollection = services.GetRequiredService<IMethodCollection>();
            foreach (var methodProvider in services.GetRequiredService<IEnumerable<IMethodProvider>>())
            {
                methodProvider.Collect(methodCollection);
            }

            return methodCollection;
        }

        private static async Task Main(string[] args)
        {
            var methodCollection = GetMethodCollection();
            await StartServer(methodCollection);
            {
                //client
                var services = new ServiceCollection()
                    .AddLogging()
                    .AddOptions()
                    .AddSingleton(methodCollection)
                    .AddGrpcClient()
                    .AddConsulDiscovery(c => c.Address = ConsulUrl)
                    .AddBreaker(options =>
                    {
                        var policy = Policy
                            .Handle<TargetInvocationException>(e =>
                            {
                                if (e.InnerException is RpcException rpcException)
                                {
                                    var statusCode = rpcException.Status.StatusCode;

                                    switch (statusCode)
                                    {
                                        case StatusCode.Unavailable:
                                        case StatusCode.DeadlineExceeded:
                                            return true;
                                    }

                                    return false;
                                }
                                return false;
                            })
                            .WaitAndRetryAsync(new[]
                            {
                                TimeSpan.FromSeconds(1),
                                TimeSpan.FromSeconds(1),
                                TimeSpan.FromSeconds(1),
                                TimeSpan.FromSeconds(1),
                                TimeSpan.FromSeconds(1)
                            });
                        options.DefaultPolicy = policy;
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
                    .UseLoadBalance()
                    .UseGrpc()
                    .Build();

                var rabbitProxyInterceptor = new GrpcProxyInterceptor(invoker);
                var proxyFactory = new ProxyFactory(rabbitProxyInterceptor);

                var service = proxyFactory.CreateInterfaceProxy<ITestService>();
                while (true)
                {
                    var t = await service.HelloAsync3(new HelloRequest { Name = "test" });
                    Console.WriteLine(t?.Message ?? "null");
                    /*service.Hello(new HelloRequest { Name = "test" });
                    await service.HelloAsync2(new HelloRequest { Name = "test" });
                    var tt = await service.HelloAsync3(new HelloRequest { Name = "test" });
                    Console.WriteLine(tt.Message);*/

                    Console.ReadLine();
                }
            }
        }
    }
}