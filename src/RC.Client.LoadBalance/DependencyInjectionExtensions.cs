using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Discovery.Abstractions;

namespace RC.Client.LoadBalance
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
    }
}