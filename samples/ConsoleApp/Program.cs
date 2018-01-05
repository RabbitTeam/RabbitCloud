using Castle.DynamicProxy;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rabbit.Cloud.Client;
using Rabbit.Cloud.Client.Go;
using Rabbit.Cloud.Client.Go.Abstractions;
using Rabbit.Cloud.Client.Go.ApplicationModels;
using Rabbit.Cloud.Client.Http;
using Rabbit.Cloud.Discovery.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class BookModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    [GoClient("http://localhost:5000/cartoons")]
    public interface ITestClient
    {
        [GoGet("/book/{query}")]
        Task<BookModel[]> SayHelloAsync([GoParameter(ParameterTarget.Path)]string query,long id=1);
    }

    public class Startup
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Run(async context =>
            {
                var result = new[]
                {
                    new BookModel
                    {
                        Id = 1,
                        Name = "book"
                    }
                };
                await context.Response.WriteAsync(JsonConvert.SerializeObject(result));
            });
        }
    }

    public class Program
    {
        private static Task RunHttpServer()
        {
            return WebHost.CreateDefaultBuilder()
                .ConfigureLogging(lb => { lb.ClearProviders(); })
                .UseStartup<Startup>()
                .UseUrls("http://localhost:5000")
                .Build()
                .RunAsync();
        }

        private class EmptyDiscoveryClient : IDiscoveryClient
        {
            #region Implementation of IDisposable

            /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
            public void Dispose()
            {
            }

            #endregion Implementation of IDisposable

            #region Implementation of IDiscoveryClient

            public string Description { get; }

            /// <summary>
            /// all serviceId
            /// </summary>
            public IReadOnlyList<string> Services { get; }

            public IReadOnlyList<IServiceInstance> GetInstances(string serviceId)
            {
                return new IServiceInstance[0];
            }

            #endregion Implementation of IDiscoveryClient
        }

        public static async Task Main(string[] args)
        {
            RunHttpServer();

            await Task.Delay(1000);
            try
            {
                var applicationServices = new ServiceCollection()
                    .AddLogging()
                    .AddOptions()
                    .AddSingleton<IDiscoveryClient, EmptyDiscoveryClient>()
                    .BuildServiceProvider();

                var services = new ServiceCollection()
                    .AddRabbitClient(applicationServices, app =>
                    {
                        app
                            .UseRabbitClient()
                            .UseRabbitHttpClient();
                    })
                    .AddSingleton(RabbitApplicationBuilder.BuildModel(new[] { typeof(ITestClient).GetTypeInfo() }))
                    .AddSingleton<IInterceptor, DefaultInterceptor>()
                    .AddGoClientProxy()
                    .BuildServiceProvider();

                var testClient = services.GetRequiredService<ITestClient>();
                var response = await testClient.SayHelloAsync("$filter=Id eq 220");
                Console.WriteLine(JsonConvert.SerializeObject(response));

                var watch=Stopwatch.StartNew();
                for (int i = 0; i < 10000; i++)
                {
                    await testClient.SayHelloAsync("$filter=Id eq 220");
                }
                watch.Stop();
                Console.WriteLine(watch.ElapsedMilliseconds+"ms");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.ReadLine();
        }
    }
}