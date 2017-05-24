using RabbitCloud.Config.Abstractions.Adapter;
using RabbitCloud.Config.Abstractions.Support;
using RabbitCloud.Rpc.Abstractions.Formatter;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RabbitCloud.Config.Internal
{
    public class DefaultFormatterFactory : IFormatterFactory
    {
        private readonly ConcurrentDictionary<string, IRequestFormatter> _requestFormatters = new ConcurrentDictionary<string, IRequestFormatter>();
        private readonly ConcurrentDictionary<string, IResponseFormatter> _responseFormatters = new ConcurrentDictionary<string, IResponseFormatter>();
        private readonly IEnumerable<IFormatterProvider> _providers;

        public DefaultFormatterFactory(IEnumerable<IFormatterProvider> providers)
        {
            _providers = providers;
        }

        #region Implementation of IFormatterFactory

        public IRequestFormatter GetRequestFormatter(string name)
        {
            return _requestFormatters.GetOrAdd(name, key => GetProvider(key).CreateRequestFormatter());
        }

        public IResponseFormatter GetResponseFormatter(string name)
        {
            return _responseFormatters.GetOrAdd(name, key => GetProvider(key).CreateResponseFormatter());
        }

        #endregion Implementation of IFormatterFactory

        private IFormatterProvider GetProvider(string name)
        {
            return _providers.SingleOrDefault(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}