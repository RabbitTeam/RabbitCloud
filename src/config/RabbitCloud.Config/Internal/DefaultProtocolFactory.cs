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
    public class DefaultProtocolFactory : IProtocolFactory
    {
        private readonly IEnumerable<IProtocolProvider> _providers;
        private readonly ConcurrentDictionary<string, IProtocol> _protocols = new ConcurrentDictionary<string, IProtocol>();

        public DefaultProtocolFactory(IEnumerable<IProtocolProvider> providers)
        {
            _providers = providers;
        }

        #region Implementation of IProtocolFactory

        public IProtocol GetProtocol(ProtocolConfig config)
        {
            return _protocols.GetOrAdd(config.Name, key => GetProvider(key).CreateProtocol(config));
        }

        #endregion Implementation of IProtocolFactory

        private IProtocolProvider GetProvider(string name)
        {
            return _providers.SingleOrDefault(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}