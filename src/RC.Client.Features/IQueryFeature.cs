using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Features
{
    public interface IQueryFeature
    {
        IDictionary<string, StringValues> Query { get; set; }
    }
}