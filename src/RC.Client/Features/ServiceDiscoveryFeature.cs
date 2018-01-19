using Rabbit.Cloud.Client.Abstractions.Features;
using Rabbit.Cloud.Discovery.Abstractions;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Features
{
    public class ServiceDiscoveryFeature : IServiceDiscoveryFeature
    {
        #region Implementation of IServiceDiscoveryFeature

        public string ServiceId { get; set; }
        public IReadOnlyList<IServiceInstance> ServiceInstances { get; set; }

        #endregion Implementation of IServiceDiscoveryFeature
    }
}