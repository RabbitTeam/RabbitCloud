using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.IO;

namespace Rabbit.Cloud.Client.Abstractions
{
    public interface IRabbitResponse
    {
        IRabbitContext RabbitContext { get; }
        int StatusCode { get; set; }
        IDictionary<string, StringValues> Headers { get; }
        Stream Body { get; set; }
    }
}