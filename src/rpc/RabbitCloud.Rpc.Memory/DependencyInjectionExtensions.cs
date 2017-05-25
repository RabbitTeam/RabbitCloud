using Microsoft.Extensions.DependencyInjection;
using RabbitCloud.Config.Abstractions.Adapter;
using RabbitCloud.Rpc.Memory.Config;

namespace RabbitCloud.Rpc.Memory
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddMemoryProtocol(this IServiceCollection services)
        {
            services
                .AddSingleton<IProtocolProvider, MemoryProtocolProvider>();

            return services;
        }
    }
}