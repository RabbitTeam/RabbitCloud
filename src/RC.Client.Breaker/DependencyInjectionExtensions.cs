using Microsoft.Extensions.DependencyInjection;
using System;

namespace Rabbit.Cloud.Client.Breaker
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddBreaker(this IServiceCollection services)
        {
            return services;
        }

        public static IServiceCollection AddBreaker(this IServiceCollection services, Action<BreakerOptions> configure)
        {
            return services
                .AddBreaker()
                .Configure(configure);
        }
    }
}