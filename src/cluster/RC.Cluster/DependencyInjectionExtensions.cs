using Microsoft.Extensions.DependencyInjection;
using RC.Abstractions;
using RC.Cluster.Abstractions.LoadBalance;
using RC.Cluster.HighAvailability;
using RC.Cluster.LoadBalance;
using System;

namespace RC.Cluster
{
    public static class DependencyInjectionExtensions
    {
        public static IRabbitBuilder AddHighAvailability(this IRabbitBuilder builder, Action<HighAvailabilityOptions> configure = null)
        {
            builder.Services.Configure(configure ?? (s => { }));
            return builder;
        }

        public static IRabbitBuilder AddRandomServiceInstanceChoose(this IRabbitBuilder builder)
        {
            return builder.AddServiceInstanceChoose<RandomServiceInstanceChoose>();
        }

        public static IRabbitBuilder AddRoundRobinServiceInstanceChoose(this IRabbitBuilder builder)
        {
            return builder.AddServiceInstanceChoose<RoundRobinServiceInstanceChoose>();
        }

        public static IRabbitBuilder AddServiceInstanceChoose<T>(this IRabbitBuilder builder) where T : class, IServiceInstanceChoose
        {
            builder.Services
                .AddSingleton<T>()
                .AddSingleton<IServiceInstanceChoose, T>();
            return builder;
        }
    }
}