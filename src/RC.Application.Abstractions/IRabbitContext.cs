using Rabbit.Cloud.Application.Features;
using System;

namespace Rabbit.Cloud.Application.Abstractions
{
    public interface IRabbitContext
    {
        IFeatureCollection Features { get; }
        IServiceProvider RequestServices { get; set; }
        IRabbitRequest Request { get; }
    }
}