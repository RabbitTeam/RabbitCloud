using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;

namespace Rabbit.Cloud.Client.Abstractions
{
    public interface IRabbitRequest
    {
        IRabbitContext RabbitContext { get; }
        Uri RequestUri { get; set; }
        IDictionary<string, StringValues> Headers { get; }
        Stream Body { get; set; }
    }
}