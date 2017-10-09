using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.IO;

namespace Rabbit.Cloud.Client.Abstractions
{
    public abstract class RabbitRequest
    {
        public abstract RabbitContext RabbitContext { get; }

        public abstract string ServiceName { get; set; }

        public abstract string Scheme { get; set; }

        public abstract string Path { get; set; }
        public abstract string QueryString { get; set; }

        public abstract IDictionary<string, StringValues> Query { get; set; }

        public abstract IDictionary<string, StringValues> Headers { get; }

        public abstract Stream Body { get; set; }
    }
}