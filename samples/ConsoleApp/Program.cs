using Grpc.Core;
using Helloworld;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Rabbit.Cloud.Client;
using Rabbit.Cloud.Client.Abstractions.Extensions;
using Rabbit.Cloud.Client.Features;
using Rabbit.Cloud.Client.Grpc;
using Rabbit.Cloud.Client.Grpc.Features;
using Rabbit.Cloud.Client.Grpc.Internal;
using Rabbit.Cloud.Grpc.Abstractions;
using Rabbit.Cloud.Grpc.Abstractions.Utilities;
using Rabbit.Cloud.Grpc.Client;
using Rabbit.Cloud.Grpc.Client.Internal;
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

        private static async Task Main(string[] args)
        {
            {
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
                var server = new Server
                {
                    Services = { builder.Build() },
                    Ports = { new ServerPort("localhost", 9908, ServerCredentials.Insecure) }
                };
                server.Start();
            }
            //            var requestMarshaller = Marshallers.Create(model => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model)), data => JsonConvert.DeserializeObject<HelloRequest>(Encoding.UTF8.GetString(data)));
            //            var responseMarshaller = Marshallers.Create(model => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model)), data => JsonConvert.DeserializeObject<HelloReply>(Encoding.UTF8.GetString(data)));
            {
                //client

                var services = new ServiceCollection()
                    .AddOptions()
                    .Configure<DefaultMethodProviderOptions>(o =>
                    {
                        o.Types = new[] { typeof(TestService) };

                        o.MarshallerFactory = type => MarshallerUtilities.CreateMarshaller(type,
                            model => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model)),
                            data => JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), type));
                    })
                    .AddSingleton<IMethodCollection, MethodCollection>()
                    .AddSingleton<IMethodProvider, DefaultMethodProvider>()
                    .AddSingleton<ChannelPool>()
                    .AddSingleton<CallInvokerPool>()
                    //                    .AddSingleton(requestMarshaller)
                    //                    .AddSingleton(responseMarshaller)
                    .BuildServiceProvider();

                var methodCollection = services.GetRequiredService<IMethodCollection>();
                foreach (var methodProvider in services.GetRequiredService<IEnumerable<IMethodProvider>>())
                {
                    methodProvider.Collect(methodCollection);
                }

                var context = new GrpcRabbitContext(new FeatureCollection());
                context.Features.Set<IRequestFeature>(new RequestFeature
                {
                    ServiceUrl = new ServiceUrl("grpc://localhost:9908/consoleapp.TestService/test")
                });

                context.Features.Set<IGrpcRequestFeature>(new GrpcRequestFeature
                {
                    Request = new HelloRequest
                    {
                        Name = "test"
                    }
                });

                var app = new RabbitApplicationBuilder<GrpcRabbitContext>(services);

                var invoker = app
                    .UseMiddleware<GrpcRabbitContext, GrpcMiddleware>()
                    .Build();

                await invoker(context);
                var response = context.Features.Get<IGrpcResponseFeature>().Response;
                var t = (AsyncUnaryCall<HelloReply>)response;
                Console.WriteLine((await t).Message);
            }
        }
    }
}