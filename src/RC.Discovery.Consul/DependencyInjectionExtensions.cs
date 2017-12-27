using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Discovery.Abstractions;
using Rabbit.Cloud.Discovery.Consul.Discovery;
using Rabbit.Cloud.Discovery.Consul.Registry;
using System;
using Rabbit.Cloud.Discovery.Consul.Internal;

namespace Rabbit.Cloud.Discovery.Consul
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection ConfigureConsul(this IServiceCollection services, Action<ConsulOptions> configure)
        {
            return services.Configure(configure);
        }

        public static IServiceCollection AddConsulDiscovery(this IServiceCollection services)
        {
            return services
                .AddSingleton<IDiscoveryClient, ConsulDiscoveryClient>();
        }

        public static IServiceCollection AddConsulRegistry(this IServiceCollection services)
        {
            return services
                .AddSingleton<IRegistryService<ConsulRegistration>, ConsulRegistryService>()
                .AddSingleton<ServiceNameResolver>();
        }
    }
}