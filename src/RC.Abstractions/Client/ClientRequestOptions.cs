using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Abstractions.Client
{
    public class ClientRequestOptions
    {
        public IDictionary<string, IConfiguration> Configurations { get; } = new Dictionary<string, IConfiguration>(StringComparer.OrdinalIgnoreCase);
    }
}