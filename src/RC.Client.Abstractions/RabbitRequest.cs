using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.IO;

namespace Rabbit.Cloud.Client.Abstractions
{
    public abstract class RabbitRequest<TContext>
    {
        public abstract TContext RabbitContext { get; }

        public abstract string ServiceName { get; set; }

        public abstract string Scheme { get; set; }

        public abstract string Path { get; set; }
        public abstract string QueryString { get; set; }

        public abstract Stream Body { get; set; }

        public abstract IDictionary<string, StringValues> Query { get; set; }
    }
}