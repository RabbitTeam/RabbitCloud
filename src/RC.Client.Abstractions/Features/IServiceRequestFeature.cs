using Rabbit.Cloud.Client.Abstractions.Codec;
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
        Type RequesType { get; set; }
        Type ResponseType { get; set; }
        ICodec Codec { get; set; }
    }
}