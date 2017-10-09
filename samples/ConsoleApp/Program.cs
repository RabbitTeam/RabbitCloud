using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Client;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Extensions;
using Rabbit.Cloud.Client.Middlewares;
using Rabbit.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace ConsoleApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build()
                .EnableTemplateSupport();

            var services = new ServiceCollection().BuildServiceProvider();
            IRabbitApplicationBuilder app = new RabbitApplicationBuilder(services);

            app
                .UseMiddleware<RequestServicesContainerMiddleware>()
                .Use(async (context, next) =>
                {
                    Console.WriteLine(context.RequestServices);
                    await next();
                });

            var invoker = app.Build();
            var rabbitContext = new DefaultRabbitContext();
            await invoker(rabbitContext);

            var response = rabbitContext.Response;
        }
    }
}