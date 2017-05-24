using Microsoft.Extensions.Logging;
using RabbitCloud.Config.Abstractions;
using RabbitCloud.Config.Abstractions.Support;
using RabbitCloud.Registry.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RabbitCloud.Config.Internal
{
    public class DefaultRegistryTableFactory : IRegistryTableFactory, IDisposable
    {
        private readonly IEnumerable<IRegistryTableProvider> _providers;
        private readonly ILogger<DefaultRegistryTableFactory> _logger;
        private readonly ConcurrentDictionary<string, Lazy<IRegistryTable>> _registryTables = new ConcurrentDictionary<string, Lazy<IRegistryTable>>();

        public DefaultRegistryTableFactory(IEnumerable<IRegistryTableProvider> providers, ILogger<DefaultRegistryTableFactory> logger)
        {
            _providers = providers;
            _logger = logger;
        }

        #region Implementation of IRegistryTableFactory

        public IRegistryTable GetRegistryTable(RegistryConfig config)
        {
            return _registryTables.GetOrAdd(config.Name, key => new Lazy<IRegistryTable>(() => GetProvider(config.Protocol).CreateRegistryTable(config))).Value;
        }

        #endregion Implementation of IRegistryTableFactory

        private IRegistryTableProvider GetProvider(string name)
        {
            return _providers.SingleOrDefault(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        #region IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            foreach (var registryTable in _registryTables.Values.Select(i => i.Value))
            {
                try
                {
                    (registryTable as IDisposable)?.Dispose();
                }
                catch (Exception e)
                {
                    _logger.LogError(0, e, $"dispose '{registryTable}' throw exception.");
                }
            }
        }

        #endregion IDisposable
    }
}