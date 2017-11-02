using Polly;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Breaker
{
    public class BreakerOptions
    {
        public Policy DefaultPolicy { get; set; }

        public IDictionary<string, Policy> Policies { get; } = new Dictionary<string, Policy>(StringComparer.OrdinalIgnoreCase);
    }
}