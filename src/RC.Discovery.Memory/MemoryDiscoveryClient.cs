using Rabbit.Cloud.Discovery.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Discovery.Memory
{
    public class MemoryDiscoveryClient : IDiscoveryClient
    {
        private readonly IEnumerable<IServiceInstance> _serviceInstances;

        public MemoryDiscoveryClient(IEnumerable<IServiceInstance> serviceInstances)
        {
            _serviceInstances = serviceInstances;
        }

        #region Implementation of IDisposable

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion Implementation of IDisposable

        #region Implementation of IDiscoveryClient

        public string Description => "Memory";

        /// <inheritdoc />
        /// <summary>
        /// all serviceId
        /// </summary>
        public IReadOnlyCollection<string> Services => _serviceInstances.Select(i => i.ServiceId).ToArray();

        public IReadOnlyCollection<IServiceInstance> GetInstances(string serviceId)
        {
            return _serviceInstances.Where(i => string.Equals(i.ServiceId, serviceId, StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        #endregion Implementation of IDiscoveryClient
    }
}