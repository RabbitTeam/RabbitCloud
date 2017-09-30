using Rabbit.Cloud.Client.Features;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Abstractions
{
    public abstract class RabbitContext<TRequest, TResponse>
    {
        public abstract IFeatureCollection Features { get; }
        public abstract TRequest Request { get; }
        public abstract TResponse Response { get; }
        public abstract IDictionary<object, object> Items { get; set; }
        public abstract IServiceProvider RequestServices { get; set; }
    }
}