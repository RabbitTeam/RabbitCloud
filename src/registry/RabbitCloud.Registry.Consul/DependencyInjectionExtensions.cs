using Microsoft.Extensions.DependencyInjection;
using RabbitCloud.Config;
using RabbitCloud.Registry.Consul.Config;

namespace RabbitCloud.Registry.Consul
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddConsulRegistryTable(this IServiceCollection services)
        {
            return services
                .AddSingleton<IRegistryTableProvider, ConsulRegistryTableProvider>();
        }
    }
}