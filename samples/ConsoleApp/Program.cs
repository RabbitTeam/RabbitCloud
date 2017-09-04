using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Discovery.Abstractions;
using Rabbit.Cloud.Extensions.Consul;
using Rabbit.Cloud.Facade;
using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Extensions.Configuration;
using System;
using System.Threading.Tasks;

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
                .AddOptions()
                .Configure<RabbitConsulOptions>(configuration.GetSection("RabbitCloud:Consul"))
                .AddConsulDiscovery()
                .BuildServiceProvider();

            var discoveryClient = services.GetRequiredService<IDiscoveryClient>();
            var client = new ProxyFactory(discoveryClient);
            var user = await client.GetProxy<IUserService>().GetUserAsync(0);

            Console.WriteLine($"name:{user.Name}");
            Console.WriteLine($"age:{user.Age}");
        }
    }
}