using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Client;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Extensions;
using Rabbit.Cloud.Client.Http;
using Rabbit.Extensions.Configuration;
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
            IRabbitApplicationBuilder<HttpRabbitContext> app = new RabbitApplicationBuilder<HttpRabbitContext>(services);

            app
                .Use(async (context, next) =>
                {
                    await next();
                });

            var invoker = app.Build();
            var rabbitContext = new HttpRabbitContext();
            await invoker(rabbitContext);

            var response = rabbitContext.Response;
        }
    }
}