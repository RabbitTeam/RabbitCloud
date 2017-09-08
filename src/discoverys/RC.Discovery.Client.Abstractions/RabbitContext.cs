using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;

namespace RC.Discovery.Client.Abstractions
{
    public abstract class RabbitContext
    {
        public abstract IFeatureCollection Features { get; }
        public RabbitRequest Request { get; set; }
        public RabbitResponse Response { get; set; }
        public abstract IDictionary<object, object> Items { get; set; }
        public IServiceProvider RequestServices { get; set; }
    }
}