using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace Rabbit.Cloud.Application.Features
{
    public interface IResponseFeature
    {
        int StatusCode { get; set; }
        IDictionary<string, StringValues> Headers { get; set; }
        object Body { get; set; }
    }
}