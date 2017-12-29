using Rabbit.Cloud.Discovery.Abstractions;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Abstractions.Features
{
    public interface IServiceRequestFeature
    {
        string ServiceName { get; set; }
        string ServiceProtocol { get; set; }
        ServiceRequestOptions RequestOptions { get; set; }
        IReadOnlyList<IServiceInstance> ServiceInstances { get; set; }
        IServiceInstanceChooser Chooser { get; set; }
        IServiceInstance ServiceInstance { get; set; }
    }
}