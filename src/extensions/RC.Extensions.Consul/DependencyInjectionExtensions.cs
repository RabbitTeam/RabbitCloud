using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Discovery.Abstractions;
using Rabbit.Cloud.Extensions.Consul.Discovery;
using Rabbit.Cloud.Extensions.Consul.Registry;
using Rabbit.Cloud.Extensions.Consul.Utilities;
using Rabbit.Cloud.Registry.Abstractions;
using System;

namespace Rabbit.Cloud.Extensions.Consul
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddConsulDiscovery(this IServiceCollection services)
        {
            return services.AddSingleton<IDiscoveryClient, ConsulDiscoveryClient>();
        }

        public static IServiceCollection AddConsulRegistry(this IServiceCollection services, ConsulClient consulClient = null)
        {
            return consulClient != null ? services.AddSingleton<IRegistryService<ConsulRegistration>>(s => new ConsulRegistryService(consulClient, s.GetRequiredService<ILogger<ConsulRegistryService>>())) : services.AddSingleton<IRegistryService<ConsulRegistration>, ConsulRegistryService>();
        }

        public static IServiceCollection AddConsulAutoRegistry(this IServiceCollection services, ConsulClient consulClient = null)
        {
            return services
                .AddConsulRegistry(consulClient)
                .AddSingleton<IStartupFilter, AutoRegistryStartupFilter>();
        }

        /// <inheritdoc />
        private class AutoRegistryStartupFilter : IStartupFilter
        {
            #region Implementation of IStartupFilter

            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            {
                return app =>
                {
                    var services = app.ApplicationServices;

                    app.ApplicationServices.GetRequiredService<IApplicationLifetime>().ApplicationStarted.Register(
                        async () =>
                        {
                            var discoveryOptions = services.GetRequiredService<IOptionsMonitor<RabbitConsulOptions>>().CurrentValue.Discovery;
                            var registryService = services.GetRequiredService<IRegistryService<ConsulRegistration>>();
                            await registryService.RegisterAsync(ConsulUtil.Create(discoveryOptions));
                        });

                    next(app);
                };
            }

            #endregion Implementation of IStartupFilter
        }
    }
}