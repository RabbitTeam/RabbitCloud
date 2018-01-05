using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Rabbit.Cloud.Client;
using Rabbit.Cloud.Client.Go;
using Rabbit.Cloud.Client.Http;
using Rabbit.Cloud.Discovery.Consul;
using System;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class BookModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    [GoClient("http://localhost:46743/cartoons")]
    public interface ITestClient
    {
        [GoGet("/book/{query}")]
        Task<BookModel[]> SayHelloAsync(string query);
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var applicationServices = new ServiceCollection()
                .AddLogging()
                .AddOptions()
                .AddConsulDiscovery(s =>
                {
                    s.Address = "http://192.168.100.150:8500";
                })
                .BuildServiceProvider();

            var services = new ServiceCollection()
                .AddRabbitClient(applicationServices, app =>
                {
                    app
                        .UseRabbitClient()
                        .UseRabbitHttpClient();
                })
                .AddGoClientProxy()
                .BuildServiceProvider();

            var testClient = services.GetRequiredService<ITestClient>();
            var tt = await testClient.SayHelloAsync("$filter=Id eq 220");
            Console.WriteLine(JsonConvert.SerializeObject(tt));
        }
    }
}