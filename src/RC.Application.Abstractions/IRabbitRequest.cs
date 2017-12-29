using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace Rabbit.Cloud.Application.Abstractions
{
    public interface IRabbitRequest
    {
        IRabbitContext RabbitContext { get; }
        string Scheme { get; set; }
        string Host { get; set; }
        int Port { get; set; }
        string Path { get; set; }
        string QueryString { get; set; }
        IDictionary<string, StringValues> Query { get; set; }
        IDictionary<string, StringValues> Headers { get; set; }
        object Body { get; set; }
    }
}