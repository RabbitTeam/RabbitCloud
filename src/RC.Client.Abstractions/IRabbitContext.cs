using Rabbit.Cloud.Client.Features;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Abstractions
{
    public interface IRabbitContext
    {
        IFeatureCollection Features { get; }
        IRabbitRequest Request { get; }
        IRabbitResponse Response { get; }
        IDictionary<object, object> Items { get; set; }
        IServiceProvider RequestServices { get; set; }
    }
}