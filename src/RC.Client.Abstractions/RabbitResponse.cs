using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.IO;

namespace Rabbit.Cloud.Client.Abstractions
{
    public abstract class RabbitResponse
    {
        public abstract RabbitContext RabbitContext { get; }
        public abstract int StatusCode { get; set; }
        public abstract IDictionary<string, StringValues> Headers { get; }
        public abstract Stream Body { get; set; }
    }
}