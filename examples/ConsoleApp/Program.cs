using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Rabbit.Cloud.Discovery.Abstractions;
using Rabbit.Cloud.Discovery.Client;
using Rabbit.Cloud.Extensions.Consul;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleApp
{
    internal class Program
    {
        private static async Task Main()
        {
            var services = new ServiceCollection()
                .AddOptions()
                .Configure<RabbitConsulOptions>(options =>
                {
                    options.Address = "http://192.168.1.150:8500";
                })
                .AddConsulDiscovery()
                .BuildServiceProvider();

            var discoveryClient = services.GetRequiredService<IDiscoveryClient>();

            var handler = new DiscoveryHttpClientHandler(discoveryClient, NullLogger<DiscoveryHttpClientHandler>.Instance);
            var httpClient = new HttpClient(handler);

            var content = await httpClient.GetStringAsync("http://userService/User/GetUser/1");

            Console.WriteLine(content);
        }
    }
}