using Rabbit.Cloud.Discovery.Abstractions;
using System;

namespace Rabbit.Cloud.Client.Abstractions.Features
{
    public interface IServiceRequestFeature
    {
        string ServiceName { get; set; }
        string ServiceProtocol { get; set; }
        ServiceRequestOptions RequestOptions { get; set; }
        Func<IServiceInstance> GetServiceInstance { get; set; }
    }
}