using Rabbit.Cloud.Discovery.Abstractions;
using System.Collections.Generic;

namespace Rabbit.Cloud.Discovery.Configuration.Internal
{
    internal class ServiceInstance : IServiceInstance
    {
        #region Implementation of IServiceInstance

        public string ServiceId { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public IDictionary<string, string> Metadata { get; set; }

        #endregion Implementation of IServiceInstance
    }
}