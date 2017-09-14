using Microsoft.Extensions.DependencyInjection;
using RC.Cluster.Abstractions.LoadBalance;
using RC.Cluster.HighAvailability;
using RC.Cluster.LoadBalance;
using System;

namespace RC.Cluster
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddHighAvailability(this IServiceCollection services, Action<HighAvailabilityOptions> configure = null)
        {
            return services.Configure(configure ?? (s => { }));
        }

        public static IServiceCollection AddRandomAddressSelector(this IServiceCollection services)
        {
            return services.AddAddressSelector<RandomAddressSelector>();
        }

        public static IServiceCollection AddRoundRobinAddressSelector(this IServiceCollection services)
        {
            return services.AddAddressSelector<RoundRobinAddressSelector>();
        }

        public static IServiceCollection AddAddressSelector<T>(this IServiceCollection services) where T : class, IAddressSelector
        {
            return services
                .AddSingleton<T>()
                .AddSingleton<IAddressSelector, T>();
        }
    }
}