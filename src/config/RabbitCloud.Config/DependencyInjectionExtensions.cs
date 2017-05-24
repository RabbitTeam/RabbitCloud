using Microsoft.Extensions.DependencyInjection;
using RabbitCloud.Config.Abstractions;
using RabbitCloud.Config.Abstractions.Support;
using RabbitCloud.Config.Internal;

namespace RabbitCloud.Config
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddRabbitCloud(this IServiceCollection services)
        {
            services
                .AddSingleton<IClusterFactory, DefaultClusterFactory>()
                .AddSingleton<IClusterProvider, DefaultClusterProvider>()
                .AddSingleton<ILoadBalanceProvider, RandomLoadBalanceProvider>()
                .AddSingleton<ILoadBalanceProvider, RoundRobinLoadBalanceProvider>()
                .AddSingleton<IHaStrategyProvider, FailfastHaStrategyProvider>()
                .AddSingleton<IHaStrategyProvider, FailoverHaStrategyProvider>();

            services
                .AddSingleton<IProtocolFactory, DefaultProtocolFactory>()
                .AddSingleton<IFormatterFactory, DefaultFormatterFactory>()
                .AddSingleton<IRegistryTableFactory, DefaultRegistryTableFactory>()
                .AddScoped<IApplicationFactory, DefaultApplicationFactory>();

            return services;
        }
    }
}