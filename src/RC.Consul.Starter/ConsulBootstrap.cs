using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rabbit.Cloud.Discovery.Consul;

namespace Rabbit.Cloud.Consul.Starter
{
    public static class ConsulBootstrap
    {
        public static void Start(IHostBuilder hostBuilder)
        {
            hostBuilder
                .ConfigureServices((ctx, services) =>
                {
                    var section = ctx.Configuration.GetSection("RabbitCloud:Consul");

                    if (!section.Exists())
                        return;

                    var client = section.GetSection("Client");
                    var instance = section.GetSection("Instance");

                    if (!client.Exists())
                        return;

                    services
                        .Configure<ConsulOptions>(client)
                        .AddConsulDiscovery()
                        .AddConsulRegistry();

                    if (!instance.Exists())
                        return;

                    services
                        .Configure<ConsulInstanceOptions>(instance)
                        .Configure<ConsulInstanceOptions>(options =>
                            {
                                if (string.IsNullOrEmpty(options.InstanceId))
                                    options.InstanceId = $"{options.Host}_{options.Port}";
                                if (string.IsNullOrEmpty(options.HealthCheckInterval))
                                    options.HealthCheckInterval = "10s";
                            })
                        .AddSingleton<IHostedService, RegistryHostedService>();
                });
        }
    }
}