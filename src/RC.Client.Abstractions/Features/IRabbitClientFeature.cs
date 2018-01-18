using System;

namespace Rabbit.Cloud.Client.Abstractions.Features
{
    public interface IRabbitClientFeature
    {
        Type RequestType { get; set; }
        Type ResponseType { get; set; }
        ServiceRequestOptions RequestOptions { get; set; }
    }
}