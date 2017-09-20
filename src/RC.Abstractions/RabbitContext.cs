using Rabbit.Cloud.Abstractions.Features;
using System;

namespace Rabbit.Cloud.Abstractions
{
    public abstract class RabbitContext
    {
        public abstract IFeatureCollection Features { get; }
        public abstract RabbitRequest Request { get; }
        public abstract RabbitResponse Response { get; }
        public abstract IServiceProvider RequestServices { get; set; }
    }
}