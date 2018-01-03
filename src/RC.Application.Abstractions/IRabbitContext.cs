using Rabbit.Cloud.Application.Features;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Application.Abstractions
{
    public interface IRabbitContext
    {
        IFeatureCollection Features { get; }
        IServiceProvider RequestServices { get; set; }
        IRabbitRequest Request { get; }
        IRabbitResponse Response { get; }
        IDictionary<object, object> Items { get; set; }
    }
}