using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Discovery.Abstractions;
using Rabbit.Cloud.Discovery.Consul.Discovery;
using Rabbit.Cloud.Discovery.Consul.Registry;
using System;

namespace Rabbit.Cloud.Discovery.Consul
{
    public static class DependencyInjectionExtensions
    {
        #region ConfigureConsul

        public static IServiceCollection ConfigureConsul(this IServiceCollection services, IConfigurationRoot configurationRoot, string sectionKey = "RabbitCloud:Consul")
        {
            return services.ConfigureConsul(configurationRoot.GetSection(sectionKey));
        }

        public static IServiceCollection ConfigureConsul(this IServiceCollection services, IConfiguration configuration)
        {
            return services.Configure<ConsulOptions>(configuration);
        }

        public static IServiceCollection ConfigureConsul(this IServiceCollection services, Action<ConsulOptions> configure)
        {
            return services.Configure(configure);
        }

        public static IServiceCollection ConfigureConsul(this IServiceCollection services, string url, string datacenter = null, string token = null)
        {
            return services.ConfigureConsul(options =>
            {
                options.Address = url;

                if (datacenter != null)
                    options.Datacenter = datacenter;
                if (token != null)
                    options.Token = token;
            });
        }

        #endregion ConfigureConsul

        public static IServiceCollection AddConsulDiscovery(this IServiceCollection services)
        {
            return services
                .AddSingleton<IDiscoveryClient, ConsulDiscoveryClient>();
        }

        public static IServiceCollection AddConsulRegistry(this IServiceCollection services)
        {
            return services
                .AddSingleton<IRegistryService<ConsulRegistration>, ConsulRegistryService>();
        }
    }
}