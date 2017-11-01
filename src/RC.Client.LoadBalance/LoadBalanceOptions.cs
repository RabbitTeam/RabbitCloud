using Rabbit.Cloud.Discovery.Abstractions;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.LoadBalance
{
    public class LoadBalanceOptions
    {
        public ILoadBalanceStrategy<string, IServiceInstance> DefaultLoadBalanceStrategy { get; set; }
        public IDictionary<string, ILoadBalanceStrategy<string, IServiceInstance>> LoadBalanceStrategies { get; } = new Dictionary<string, ILoadBalanceStrategy<string, IServiceInstance>>(StringComparer.OrdinalIgnoreCase);
    }
}