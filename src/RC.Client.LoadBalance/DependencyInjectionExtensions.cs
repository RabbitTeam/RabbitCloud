using Microsoft.Extensions.DependencyInjection;
using System;

namespace Rabbit.Cloud.Client.LoadBalance
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddLoadBalance(this IServiceCollection services)
        {
            return services.Configure<LoadBalanceOptions>(options =>
           {
               options.ServiceInstanceChooserCollection.Set("Random", new RandomServiceInstanceChooser());
               options.ServiceInstanceChooserCollection.Set("RoundRobin", new RoundRobinLoadBalanceStrategy());
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