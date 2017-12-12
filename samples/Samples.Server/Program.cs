using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rabbit.Cloud.Grpc.Abstractions.Server;
using Rabbit.Extensions.Boot;
using Samples.Service;
using System;
using System.Threading.Tasks;

namespace Samples.Server
{
    public class ApplicationInfo
    {
        public string ServiceName { get; set; }
        public string HostName { get; set; }
        public int HostPort { get; set; }
    }

    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var hostBuilder = await RabbitBoot.BuildHostBuilderAsync(builder =>
            {
                builder
                .ConfigureHostConfiguration(b => b.AddJsonFile("appsettings.json"))
                .ConfigureServices(s =>
                {
                    s
                        .AddLogging()
                        .AddOptions()
                        .AddSingleton<ITestService, TestService>();
                });
            });
            var host = hostBuilder.Build();

            await host.StartAsync();

            Console.WriteLine("press key exit...");
            Console.ReadLine();
            /*var host=hostBuilder.Build();
            ApplicationInfo = BuildConfiguration(args).Get<ApplicationInfo>();
            ApplicationInfo.HostName = "192.168.18.190";
            ApplicationInfo.HostPort = 9908;
            {
                IServiceProvider services = new ServiceCollection()
                    .AddLogging()
                    .AddOptions()
                    .AddSingleton<ITestService, TestService>()
                    .Configure<ConsulOptions>(s =>
                    {
                        s.Address = "http://192.168.100.150:8500";
                    })
                    .Configure<ConsulDiscoveryOptions>(s =>
                    {
                        s.HostName = "192.168.18.190";
                        s.InstanceId = "192.168.18.190_9908";
                        s.Port = 9908;
                        s.ServiceName = "Samples.Service";
                        s.HealthCheckInterval = "10s";
                    })
                    .AddConsulRegistry()
                    .AddConsulDiscovery()
                    .AddGrpcServer(options =>
                    {
                        options
                            .Serializers
                            .AddProtobufSerializer()
                            .AddMessagePackSerializer()
                            .AddJsonSerializer();
                    })
                    .AddServerGrpc()
                    .BuildServiceProvider();

                var registryService = services.GetRequiredService<IRegistryService<ConsulRegistration>>();
                await registryService.RegisterAsync(
                    ConsulUtil.Create(services.GetRequiredService<IOptions<ConsulDiscoveryOptions>>().Value));

                var serverServiceDefinitionTable = services.GetRequiredService<IServerServiceDefinitionTableProvider>().ServerServiceDefinitionTable;

                {
                    var server = new Grpc.Core.Server
                    {
                        Ports = { new ServerPort(ApplicationInfo.HostName, ApplicationInfo.HostPort, ServerCredentials.Insecure) }
                    };

                    foreach (var definition in serverServiceDefinitionTable)
                    {
                        server.Services.Add(definition);
                    }
                    server.Start();
                }

                Console.WriteLine("press key exit...");
                Console.ReadLine();*/
        }

        private static IConfiguration BuildConfiguration(string[] args)
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
        }
    }
}