using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Discovery.Abstractions;
using System;

namespace Rabbit.Cloud.Client.LoadBalance
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddLoadBalance(this IServiceCollection services)
        {
            return services.Configure<LoadBalanceOptions>(options =>
            {
                options.DefaultLoadBalanceStrategy = options.LoadBalanceStrategies["Random"] = new RandomLoadBalanceStrategy<string, IServiceInstance>();
                options.LoadBalanceStrategies["RoundRobin"] = new RoundRobinLoadBalanceStrategy<string, IServiceInstance>();
            });
        }

        public static IServiceCollection AddLoadBalance(this IServiceCollection services, Action<LoadBalanceOptions> configure)
        {
            return services
                .AddLoadBalance()
                .Configure(configure);
        }
    }
}