using Grpc.Core;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Application.Abstractions.Extensions;
using Rabbit.Cloud.Client;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Grpc;
using Rabbit.Cloud.Discovery.Consul;
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
    internal class UserModel
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public enum CartoonBookResourcesType
    {
        /// <summary>
        /// zip
        /// </summary>
        Zip = 0,

        /// <summary>
        /// 妖气web
        /// </summary>
        U17Web = 1,

        /// <summary>
        /// 未知
        /// </summary>
        Unknown = 2
    }

    public class CartoonRecommendFilter
    {
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
        public int? Type { get; set; }
        public int? Position { get; set; }
        public CartoonBookResourcesType[] ResourcesTypes { get; set; }
    }

    public class CartoonRecommendModel
    {
        public long BookId { get; set; }
        public int Position { get; set; }
        public int Type { get; set; }

        public CartoonBookResourcesType ResourcesType { get; set; }
    }

    public class CartoonRecommendResponse
    {
        public long TotalCount { get; set; }
        public IReadOnlyList<CartoonRecommendModel> Data { get; set; }
    }

    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.Run(async c =>
            {
                await c.Response.WriteAsync(JsonConvert.SerializeObject(new UserModel
                {
                    Age = 20,
                    Name = "ben"
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
            var requestMarshaller = Marshallers.Create(s => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(s)),
                data => JsonConvert.DeserializeObject<CartoonRecommendFilter>(Encoding.UTF8.GetString(data)));
            var responseMarshaller = Marshallers.Create(s => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(s)),
                data => JsonConvert.DeserializeObject<CartoonRecommendResponse>(Encoding.UTF8.GetString(data)));

            context.Results.Add(new Method<CartoonRecommendFilter, CartoonRecommendResponse>(MethodType.Unary, "CartoonRecommendService", "GetRecommendBooksAsync", requestMarshaller, responseMarshaller));
        }

        public void OnProvidersExecuted(MethodProviderContext context)
        {
        }

        #endregion Implementation of IMethodProvider
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            Task.Run(() =>
            {
                WebHost.CreateDefaultBuilder()
                    .UseStartup<Startup>()
                    .Build().StartAsync();
            });

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
                .UseMiddleware<PreClientMiddleware>()
                .UseMiddleware<RequestOptionMiddleware>()
                .UseMiddleware<ServiceInstanceMiddleware>()
                .UseMiddleware<ClientMiddleware>()
                .Use(async (c, n) =>
                {
                    /*                    c.Features.Get<IServiceRequestFeature>().ServiceInstance = new ConsulServiceInstance
                                        {/*
                                            Host = "localhost",
                                            Port = 5000#1#
                                            Host = "192.168.100.150",
                                            Port = 9903
                                        };*/

                    await n();
                })
                .UseMiddleware<GrpcMiddleware>()
                //                .UseMiddleware<HttpMiddleware>()
                .Build();

            var rabbitClient = new RabbitClient(app, services);

            Task.Run(async () =>
            {
                var response = await rabbitClient.SendAsync<CartoonRecommendFilter, CartoonRecommendResponse>(
                    "grpc://Cartoon/CartoonRecommendService/GetRecommendBooksAsync?a=1",
                    new CartoonRecommendFilter());
                Console.WriteLine(JsonConvert.SerializeObject(response));
            }).GetAwaiter().GetResult();
        }
    }
}