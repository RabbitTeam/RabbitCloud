using Consul;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitCloud.Config;
using RabbitCloud.Config.Abstractions;
using RabbitCloud.Registry.Abstractions;
using System;
using System.Collections.Concurrent;

namespace RabbitCloud.Registry.Consul.Config
{
    public class ConsulRegistryTableProvider : IRegistryTableProvider
    {
        private readonly IServiceProvider _container;
        private readonly ConcurrentDictionary<string, ConsulClient> _consulClients = new ConcurrentDictionary<string, ConsulClient>();

        public ConsulRegistryTableProvider(IServiceProvider container)
        {
            _container = container;
        }

        #region Implementation of IRegistryTableProvider

        public string Name { get; } = "Consul";

        public IRegistryTable CreateRegistryTable(RegistryConfig config)
        {
            var consulClient = _consulClients.GetOrAdd(config.Address, k =>
            {
                return new ConsulClient(options =>
                {
                    options.Address = new Uri(config.Address);
                });
            });

            return new ConsulRegistryTable(consulClient, new HeartbeatManager(consulClient), _container.GetRequiredService<ILogger<ConsulRegistryTable>>());
        }

        #endregion Implementation of IRegistryTableProvider
    }
}