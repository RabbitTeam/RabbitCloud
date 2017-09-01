using Consul;
using Microsoft.Extensions.Logging.Abstractions;
using Rabbit.Cloud.Discovery.Client;
using Rabbit.Cloud.Extensions.Consul.Discovery;
using Rabbit.Cloud.Extensions.Consul.Registry;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var consulClient = new ConsulClient(s =>
            {
                s.Address = new Uri("http://192.168.1.150:8500");
            });

            var response = await consulClient.Agent.Services();
            foreach (var item in response.Response.Values)
            {
                await consulClient.Agent.ServiceDeregister(item.ID);
            }

            var handler = new DiscoveryHttpClientHandler(new ConsulDiscoveryClient(consulClient), NullLogger<DiscoveryHttpClientHandler>.Instance);
            var consulRegistryService = new ConsulRegistryService(consulClient);
            await consulRegistryService.RegisterAsync(ConsulRegistration.Create("userService", new Uri("http://localhost:20578")));

            var httpClient = new HttpClient(handler);

            var content = await httpClient.GetStringAsync("http://userService/User/GetUser/1");
            Console.WriteLine(content);

            Console.ReadLine();
        }
    }
}