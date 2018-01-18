using Rabbit.Cloud.Discovery.Abstractions;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Abstractions.Features
{
    public interface IServiceDiscoveryFeature
    {
        string ServiceId { get; set; }
        IReadOnlyList<IServiceInstance> ServiceInstances { get; set; }
    }
}