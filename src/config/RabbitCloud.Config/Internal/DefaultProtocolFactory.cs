using Microsoft.Extensions.Logging;
using RabbitCloud.Config.Abstractions;
using RabbitCloud.Config.Abstractions.Adapter;
using RabbitCloud.Config.Abstractions.Support;
using RabbitCloud.Rpc.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RabbitCloud.Config.Internal
{
    public class DefaultProtocolFactory : IProtocolFactory, IDisposable
    {
        private readonly IEnumerable<IProtocolProvider> _providers;
        private readonly ILogger<DefaultProtocolFactory> _logger;
        private readonly ConcurrentDictionary<string, Lazy<IProtocol>> _protocols = new ConcurrentDictionary<string, Lazy<IProtocol>>();

        public DefaultProtocolFactory(IEnumerable<IProtocolProvider> providers, ILogger<DefaultProtocolFactory> logger)
        {
            _providers = providers;
            _logger = logger;
        }

        #region Implementation of IProtocolFactory

        public IProtocol GetProtocol(ProtocolConfig config)
        {
            return _protocols.GetOrAdd(config.Name, key => new Lazy<IProtocol>(() => GetProvider(key).CreateProtocol(config))).Value;
        }

        #endregion Implementation of IProtocolFactory

        private IProtocolProvider GetProvider(string name)
        {
            return _providers.SingleOrDefault(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        #region IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            foreach (var protocol in _protocols.Values.Select(i => i.Value))
            {
                try
                {
                    protocol.Dispose();
                }
                catch (Exception e)
                {
                    _logger.LogError(0, e, $"dispose '{protocol}' throw exception.");
                }
            }
        }

        #endregion IDisposable
    }
}