using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabbit.Cloud.Discovery.Abstractions;
using Rabbit.Cloud.Discovery.Consul.Discovery;
using Rabbit.Cloud.Discovery.Consul.Registry;
using System;

namespace Rabbit.Cloud.Discovery.Consul
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddConsulDiscovery(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration is IConfigurationRoot configurationRoot)
                configuration = configurationRoot.GetSection("RabbitCloud:Consul");

            services
                .Configure<RabbitConsulOptions>(configuration)
                .AddSingleton<IDiscoveryClient, ConsulDiscoveryClient>();

            return services;
        }

        public static IServiceCollection AddConsulDiscovery(this IServiceCollection services, Action<RabbitConsulOptions> configure)
        {
            services
                .Configure(configure)
                .AddSingleton<IDiscoveryClient, ConsulDiscoveryClient>();

            return services;
        }

        public static IServiceCollection AddConsulRegistry(this IServiceCollection services, ConsulClient consulClient = null)
        {
            if (consulClient == null)
                services.AddSingleton<IRegistryService<ConsulRegistration>, ConsulRegistryService>();
            else
                services.AddSingleton<IRegistryService<ConsulRegistration>>(s => new ConsulRegistryService(consulClient, s.GetRequiredService<ILoggerFactory>()));
            return services;
        }
    }
}