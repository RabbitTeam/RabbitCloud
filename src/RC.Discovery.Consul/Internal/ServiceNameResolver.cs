using Microsoft.Extensions.Options;
using System;

namespace Rabbit.Cloud.Discovery.Consul.Internal
{
    public class ServiceNameResolver
    {
        private readonly ConsulOptions _options;

        public ServiceNameResolver(IOptions<ConsulOptions> options)
        {
            _options = options.Value;
        }

        public string GetConsulNameByLocalName(string localName)
        {
            if (string.IsNullOrEmpty(_options.Prefix) || localName.StartsWith(_options.Prefix, StringComparison.OrdinalIgnoreCase))
                return localName;

            return _options.Prefix + localName;
        }

        public string GetLocalNameByConsulName(string consulName)
        {
            if (string.IsNullOrEmpty(_options.Prefix) || !consulName.StartsWith(_options.Prefix, StringComparison.OrdinalIgnoreCase))
                return consulName;

            return consulName.Substring(_options.Prefix.Length);
        }
    }
}