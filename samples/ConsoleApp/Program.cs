using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Rabbit.Cloud.Discovery.Client.Internal;
using Rabbit.Cloud.Discovery.Client.Middlewares;
using Rabbit.Cloud.Extensions.Consul;
using Rabbit.Cloud.Facade;
using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Builder;
using Rabbit.Extensions.Configuration;
using RC.Discovery.Client.Abstractions;
using RC.Discovery.Client.Abstractions.Extensions;
using RC.Facade.Formatters.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using ProxyFactory = Rabbit.Cloud.Facade.ProxyFactory;

namespace ConsoleApp
{
    public class UserMode
    {
        public string Name { get; set; }
        public ushort Age { get; set; }
    }

    [FacadeClient("userService")]
    public interface IUserService
    {
        [RequestMapping]
        Task<UserMode> GetUserAsync(long id);

        [RequestMapping]
        UserMode GetUser(long id);
    }

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

            var services = new ServiceCollection()
                .AddSingleton(new HttpClient())
                .Configure<RabbitConsulOptions>(configuration.GetSection("RabbitCloud:Consul"))
                .AddConsulDiscovery()
                .AddFacadeCore()
                .AddJsonFormatters()
                .Services
                .BuildServiceProvider();

            IRabbitApplicationBuilder applicationBuilder = new RabbitApplicationBuilder(services);

            applicationBuilder
                .UseFacade()
                .UseMiddleware(typeof(ServiceAddressResolveMiddleware));

            var rabbitRequestDelegate = applicationBuilder.Build();

            var proxyFactory = new ProxyFactory(rabbitRequestDelegate, services.GetRequiredService<IOptions<FacadeOptions>>());

            var userService = proxyFactory.GetProxy<IUserService>();

            var model = await userService.GetUserAsync(1);

            Console.WriteLine(JsonConvert.SerializeObject(model));
        }
    }
}