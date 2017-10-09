using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.IO;

namespace Rabbit.Cloud.Client.Features
{
    public interface IResponseFeature
    {
        int StatusCode { get; set; }
        IDictionary<string, StringValues> Headers { get; set; }
        Stream Body { get; set; }
        bool HasStarted { get; }
    }
}