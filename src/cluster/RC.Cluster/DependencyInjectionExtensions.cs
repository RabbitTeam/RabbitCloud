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

        public static IServiceCollection AddRandomServiceInstanceChoose(this IServiceCollection services)
        {
            return services.AddServiceInstanceChoose<RandomServiceInstanceChoose>();
        }

        public static IServiceCollection AddRoundRobinServiceInstanceChoose(this IServiceCollection services)
        {
            return services.AddServiceInstanceChoose<RoundRobinServiceInstanceChoose>();
        }

        public static IServiceCollection AddServiceInstanceChoose<T>(this IServiceCollection services) where T : class, IServiceInstanceChoose
        {
            return services
                .AddSingleton<T>()
                .AddSingleton<IServiceInstanceChoose, T>();
        }
    }
}