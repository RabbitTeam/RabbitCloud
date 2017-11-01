using Grpc.Core;
using Helloworld;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Rabbit.Cloud.Client;
using Rabbit.Cloud.Client.Features;
using Rabbit.Cloud.Client.Grpc;
using Rabbit.Cloud.Client.Grpc.Builder;
using Rabbit.Cloud.Discovery.Consul;
using Rabbit.Cloud.Grpc.Abstractions;
using Rabbit.Cloud.Grpc.Abstractions.Method;
using Rabbit.Cloud.Grpc.Abstractions.Utilities;
using Rabbit.Cloud.Grpc.Client;
using RC.Client.LoadBalance;
using RC.Client.LoadBalance.Builder;
using System;
using System.Collections.Generic;
using System.Text;
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

        private static void StartServer()
        {
            {
                /*                var services = new ServiceCollection()
                                    .AddLogging()
                                    .AddConsulRegistry(new ConsulClient(c => c.Address = new Uri("http://192.168.1.150:8500")))
                                    .BuildServiceProvider();

                                var registryService = services.GetRequiredService<IRegistryService<ConsulRegistration>>();
                                var consulRegistration = new ConsulRegistration(new AgentServiceRegistration
                                {
                                    Address = "localhost",
                                    ID = "rabbitcloud:test_localhost_9908",
                                    Name = "testservice",
                                    Port = 9908
                                });
                                await registryService.RegisterAsync(consulRegistration);
                                consulRegistration.AgentServiceRegistration.ID = "rabbitcloud:test_localhost_9907";
                                consulRegistration.AgentServiceRegistration.Port = 9907;
                                await registryService.RegisterAsync(consulRegistration);*/
                //server
                var methodInfo = typeof(TestService).GetMethod(nameof(TestService.SayHello));
                var descriptor = GrpcServiceDescriptor.Create(methodInfo);

                var methodDef = (Method<HelloRequest, HelloReply>)MethodUtilities.CreateMethod(descriptor.ServiceName,
                    "test", descriptor.MethodType, descriptor.RequesType, descriptor.ResponseType,
                    MarshallerUtilities.CreateMarshaller(typeof(HelloRequest),
                        model => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model)),
                        data => JsonConvert.DeserializeObject<HelloRequest>(Encoding.UTF8.GetString(data))),
                    MarshallerUtilities.CreateMarshaller(typeof(HelloReply),
                        model => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model)),
                        data => JsonConvert.DeserializeObject<HelloReply>(Encoding.UTF8.GetString(data))));

                var builder = ServerServiceDefinition.CreateBuilder();
                builder.AddMethod(methodDef, new TestService().SayHello);
                var serverServiceDefinition = builder.Build();
                {
                    var server = new Server
                    {
                        Services = { serverServiceDefinition },
                        Ports = { new ServerPort("localhost", 9908, ServerCredentials.Insecure) }
                    };
                    server.Start();
                }
                {
                    var server = new Server
                    {
                        Services = { serverServiceDefinition },
                        Ports = { new ServerPort("localhost", 9907, ServerCredentials.Insecure) }
                    };
                    server.Start();
                }
            }
        }

        private static async Task Main(string[] args)
        {
            StartServer();
            {
                //client
                var services = new ServiceCollection()
                    .AddLogging()
                    .AddOptions()
                    .AddGrpcCore(options =>
                    {
                        options.Types = new[] { typeof(TestService) };

                        options.MarshallerFactory = type => MarshallerUtilities.CreateMarshaller(type,
                            model => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model)),
                            data => JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), type));
                    })
                    .AddGrpcClient()
                    .AddConsulDiscovery(c => c.Address = "http://192.168.1.150:8500")
                    .AddLoadBalance()
                    .BuildServiceProvider();

                var methodCollection = services.GetRequiredService<IMethodCollection>();
                foreach (var methodProvider in services.GetRequiredService<IEnumerable<IMethodProvider>>())
                {
                    methodProvider.Collect(methodCollection);
                }

                var context = new GrpcRabbitContext();
                context.Request.Url = new ServiceUrl("grpc://TestService/consoleapp.TestService/test");
                context.Request.Request = new HelloRequest
                {
                    Name = "test"
                };

                var app = new RabbitApplicationBuilder(services);

                var invoker = app
                    .UseLoadBalance()
                    .UseGrpc()
                    .Build();

                while (true)
                {
                    await invoker(context);
                    var response = context.Response.Response;
                    var t = (AsyncUnaryCall<HelloReply>)response;
                    Console.WriteLine((await t).Message);
                    Console.ReadLine();
                }
            }
        }
    }
}