using Rabbit.Cloud.Client.Features;
using System;

namespace Rabbit.Cloud.Client.Abstractions
{
    public interface IRabbitContext
    {
        IFeatureCollection Features { get; }
        IServiceProvider RequestServices { get; set; }
        IRabbitRequest Request { get; }
    }
}