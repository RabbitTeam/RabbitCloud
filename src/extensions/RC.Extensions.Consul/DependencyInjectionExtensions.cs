using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Abstractions;
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
        public static IRabbitBuilder AddConsulDiscovery(this IRabbitBuilder builder)
        {
            builder.Services.AddSingleton<IDiscoveryClient, ConsulDiscoveryClient>();
            return builder;
        }

        public static IRabbitBuilder AddConsulRegistry(this IRabbitBuilder builder, ConsulClient consulClient = null)
        {
            var services = builder.Services;
            if (consulClient == null)
                services.AddSingleton<IRegistryService<ConsulRegistration>, ConsulRegistryService>();
            else
                services.AddSingleton<IRegistryService<ConsulRegistration>>(s => new ConsulRegistryService(consulClient, s.GetRequiredService<ILoggerFactory>()));
            return builder;
        }

        public static IRabbitBuilder AddConsulAutoRegistry(this IRabbitBuilder builder, ConsulClient consulClient = null)
        {
            builder
                .AddConsulRegistry(consulClient)
                .Services
                .AddSingleton<IStartupFilter, AutoRegistryStartupFilter>();

            return builder;
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