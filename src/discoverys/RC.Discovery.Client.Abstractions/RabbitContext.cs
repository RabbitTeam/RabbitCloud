using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;

namespace RC.Discovery.Client.Abstractions
{
    public abstract class RabbitContext
    {
        public abstract IFeatureCollection Features { get; }
        public abstract RabbitRequest Request { get; }
        public abstract RabbitResponse Response { get; }
        public abstract IDictionary<object, object> Items { get; set; }
        public abstract IServiceProvider RequestServices { get; set; }
    }
}