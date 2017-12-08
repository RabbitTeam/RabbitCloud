using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Abstractions;
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
/*
        public static IServiceCollection AddBackoff(this IServiceCollection services)
        {
            return services
                .AddBackoff(context => TimeSpan.FromSeconds(10 + (context.Entry.Count - 1) * 5));
        }

        public static IServiceCollection AddBackoff(this IServiceCollection services, TimeSpan tryInterval)
        {
            return services
                .AddBackoff(context => tryInterval);
        }

        public static IServiceCollection AddBackoff(this IServiceCollection services, Func<FailureContext, TimeSpan> tryIntervalFactory)
        {
            return services
                .AddBackoff(options =>
                {
                    options.ErrorHandler = context =>
                    {
                        if (!(context.Exception is RabbitRpcException rpcException) || rpcException.IsBusiness())
                            return;

                        var entry = context.Entry;
                        entry.Mark(tryIntervalFactory(context));
                    };
                });
        }

        public static IServiceCollection AddBackoff(this IServiceCollection services, Action<BackoffOptions> configure)
        {
            return services
                .AddSingleton<FailureTable, FailureTable>()
                .Configure(configure);
        }*/
    }
}