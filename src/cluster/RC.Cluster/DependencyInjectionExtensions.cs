using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Abstractions;
using Rabbit.Cloud.Cluster.Abstractions.LoadBalance;
using Rabbit.Cloud.Cluster.HighAvailability;
using Rabbit.Cloud.Cluster.LoadBalance;
using Rabbit.Extensions.DependencyInjection.Builder;
using System;

namespace Rabbit.Cloud.Cluster
{
    public static class DependencyInjectionExtensions
    {
        public static IRabbitBuilder AddHighAvailability(this IRabbitBuilder builder, Action<HighAvailabilityOptions> configure = null)
        {
            builder.Services
                .Configure(configure ?? (s => { }));
            return builder;
        }

        public static IRabbitBuilder AddServiceInstanceChoose(this IRabbitBuilder builder)
        {
            var containerBuilder = new RabbitContainerBuilder();
            containerBuilder.RegisterType<RandomServiceInstanceChoose>()
                .As<IServiceInstanceChoose>()
                .Named<IServiceInstanceChoose>("Random");
            containerBuilder.RegisterType<RoundRobinServiceInstanceChoose>()
                .Named<IServiceInstanceChoose>("RoundRobin");

            containerBuilder.Build(builder.Services);

            return builder;
        }
    }
}