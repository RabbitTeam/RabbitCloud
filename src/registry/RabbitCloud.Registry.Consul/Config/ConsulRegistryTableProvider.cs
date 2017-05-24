using Consul;
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
        private readonly ILogger<ConsulRegistryTable> _consulRegistryTableLogger;
        private readonly ConcurrentDictionary<string, ConsulClient> _consulClients = new ConcurrentDictionary<string, ConsulClient>();

        public ConsulRegistryTableProvider(ILogger<ConsulRegistryTable> consulRegistryTableLogger)
        {
            _consulRegistryTableLogger = consulRegistryTableLogger;
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
            var heartbeatManager = new HeartbeatManager(consulClient);
            return new ConsulRegistryTable(consulClient, heartbeatManager, _consulRegistryTableLogger);
        }

        #endregion Implementation of IRegistryTableProvider
    }
}