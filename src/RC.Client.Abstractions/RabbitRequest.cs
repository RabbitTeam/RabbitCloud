using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;

namespace Rabbit.Cloud.Client.Abstractions
{
    public abstract class RabbitRequest
    {
        public abstract RabbitContext RabbitContext { get; }
        public abstract string Method { get; set; }
        public abstract Uri RequestUri { get; set; }

        public abstract IDictionary<string, StringValues> Headers { get; }

        public abstract Stream Body { get; set; }
    }
}