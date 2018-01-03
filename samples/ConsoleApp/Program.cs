using Grpc.Core;
using Helloworld;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Abstractions.Extensions;
using Rabbit.Cloud.Client;
using Rabbit.Cloud.Client.Abstractions.Features;
using Rabbit.Cloud.Client.Codec;
using Rabbit.Cloud.Client.Grpc;
using Rabbit.Cloud.Client.Http;
using Rabbit.Cloud.Discovery.Consul;
using Rabbit.Cloud.Discovery.Consul.Discovery;
using Rabbit.Cloud.Grpc.Abstractions;
using Rabbit.Cloud.Grpc.Abstractions.Client;
using Rabbit.Cloud.Grpc.Client;
using Rabbit.Cloud.Grpc.Client.Internal;
using Rabbit.Cloud.Grpc.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.Run(async c =>
            {
                var buffer = new byte[c.Request.ContentLength.Value];
                c.Request.Body.Read(buffer, 0, buffer.Length);
                var request = JsonConvert.DeserializeObject<HelloRequest>(Encoding.UTF8.GetString(buffer));
                await c.Response.WriteAsync(JsonConvert.SerializeObject(new HelloReply
                {
                    Message = $"hello,{request.Name},from http server."
                }));
            });
        }
    }

    internal class MethodProvider : IMethodProvider
    {
        #region Implementation of IMethodProvider

        public int Order { get; }

        public void OnProvidersExecuting(MethodProviderContext context)
        {
            context.Results.Add(Greeter.__Method_SayHello);
        }

        public void OnProvidersExecuted(MethodProviderContext context)
        {
        }

        #endregion Implementation of IMethodProvider
    }

    internal class GreeterImpl : Greeter.GreeterBase
    {
        #region Overrides of GreeterBase

        /// <inheritdoc />
        /// <summary>
        /// Sends a greeting
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = $"hello,{request.Name},from grpc server."
            });
        }

        #endregion Overrides of GreeterBase
    }

    public class Program
    {
        private static Task StartGrpcServer()
        {
            var server = new Server
            {
                Services = { Greeter.BindService(new GreeterImpl()) },
                Ports = { new ServerPort("0.0.0.0", 9999, ServerCredentials.Insecure) }
            };
            server.Start();
            return server.ShutdownTask;
        }

        private static Task StartHttpServer()
        {
            return WebHost.CreateDefaultBuilder()
                .UseUrls("http://192.168.18.190:5000")
                .UseStartup<Startup>()
                .Build()
                .StartAsync();
        }

        public static async Task Main(string[] args)
        {
            StartGrpcServer();
            StartHttpServer();

            await Task.Delay(1000);
            var services = new ServiceCollection()
                .AddSingleton<ICallInvokerFactory, CallInvokerFactory>()
                .AddSingleton<IMethodTableProvider, DefaultMethodTableProvider>()
                .AddSingleton<IMethodProvider, MethodProvider>()
                .AddSingleton<ChannelPool>()
                .AddLogging()
                .AddOptions()
                .ConfigureConsul(s =>
                {
                    s.Address = "http://192.168.100.150:8500";
                })
                .AddConsulDiscovery()
                .BuildServiceProvider();

            var appBuild = new RabbitApplicationBuilder(services);
            var app = appBuild
                .UseMiddleware<RequestOptionMiddleware>()
                //                .UseMiddleware<ServiceInstanceMiddleware>()
                .Use(async (c, n) =>
                {
                    var feature = c.Features.Get<IServiceRequestFeature>();

                    var instance = new ConsulServiceInstance
                    {
                        Host = "192.168.18.190",
                        Port = c.Request.Scheme == "http" ? 5000 : 9999,
                        ServiceId = Greeter.__Method_SayHello.FullName
                    };
                    feature.GetServiceInstance = () => instance;
                    await n();
                })
                .MapWhen<IRabbitContext>(c => c.Request.Scheme == "http", ab =>
                {
                    ab
                    .Use(async (c, n) =>
                        {
                            var feature = c.Features.Get<IServiceRequestFeature>();
                            feature.Codec = new SerializerCodec(new Rabbit.Cloud.Client.Serialization.JsonSerializer(), feature.RequesType, feature.ResponseType);
                            await n();
                        })
                        .UseMiddleware<ClientMiddleware>()
                        .UseMiddleware<HttpMiddleware>();
                })
                .MapWhen<IRabbitContext>(c => c.Request.Scheme == "grpc", ab =>
                {
                    ab
                        .UseMiddleware<ClientMiddleware>()
                        .UseMiddleware<PreGrpcMiddleware>()
                        .UseMiddleware<GrpcMiddleware>();
                })
                .Build();

            var rabbitClient = new RabbitClient(app, services);

            var response = await rabbitClient.SendAsync<HelloRequest, HelloReply>(
                $"grpc://Test{Greeter.__Method_SayHello.FullName}",
                new HelloRequest { Name = "ben" });
            Console.WriteLine(JsonConvert.SerializeObject(response));

            response = await rabbitClient.SendAsync<HelloRequest, HelloReply>(
                $"http://Test{Greeter.__Method_SayHello.FullName}",
                new HelloRequest { Name = "ben" }, new Dictionary<string, StringValues>
                {
                    {"Content-Type","application/json" }
                });
            Console.WriteLine(JsonConvert.SerializeObject(response));
        }
    }
}