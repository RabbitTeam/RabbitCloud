using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace Rabbit.Cloud.Application.Abstractions
{
    public interface IRabbitResponse
    {
        IRabbitContext RabbitContext { get; }
        int StatusCode { get; set; }
        IDictionary<string, StringValues> Headers { get; set; }
        object Body { get; set; }
    }
}