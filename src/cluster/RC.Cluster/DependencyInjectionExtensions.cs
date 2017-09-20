using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Abstractions;
using Rabbit.Cloud.Cluster.Abstractions.LoadBalance;
using Rabbit.Cloud.Cluster.HighAvailability;
using Rabbit.Cloud.Cluster.LoadBalance;
using System;

namespace Rabbit.Cloud.Cluster
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