using RabbitCloud.Registry.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RabbitCloud.Config.Abstractions.Adapter
{
    public interface IRegistryTableFactory
    {
        IRegistryTable GetRegistryTable(RegistryConfig config);
    }

    public class DefaultRegistryTableFactory : IRegistryTableFactory
    {
        private readonly IEnumerable<IRegistryTableProvider> _providers;
        private readonly ConcurrentDictionary<string, IRegistryTable> _registryTables = new ConcurrentDictionary<string, IRegistryTable>();

        public DefaultRegistryTableFactory(IEnumerable<IRegistryTableProvider> providers)
        {
            _providers = providers;
        }

        #region Implementation of IRegistryTableFactory

        public IRegistryTable GetRegistryTable(RegistryConfig config)
        {
            return _registryTables.GetOrAdd(config.Name, key => GetProvider(config.Protocol).CreateRegistryTable(config));
        }

        #endregion Implementation of IRegistryTableFactory

        private IRegistryTableProvider GetProvider(string name)
        {
            return _providers.SingleOrDefault(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}