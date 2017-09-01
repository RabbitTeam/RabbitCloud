using Rabbit.Cloud.Discovery.Abstractions;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Extensions.Consul.Discovery
{
    public class ConsulServiceInstance : IServiceInstance
    {
        #region Implementation of IServiceInstance

        public string ServiceId { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool IsSecure { get; set; }
        public Uri Uri { get; set; }
        public IDictionary<string, string> Metadata { get; set; }

        #endregion Implementation of IServiceInstance
    }
}