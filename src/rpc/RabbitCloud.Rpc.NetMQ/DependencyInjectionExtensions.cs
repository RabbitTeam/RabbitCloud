using Microsoft.Extensions.DependencyInjection;
using RabbitCloud.Config.Abstractions.Adapter;
using RabbitCloud.Rpc.NetMQ.Config;

namespace RabbitCloud.Rpc.NetMQ
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddNetMqProtocol(this IServiceCollection services)
        {
            services
                .AddSingleton<IProtocolProvider, NetMqProtocolProvider>();

            return services;
        }
    }
}