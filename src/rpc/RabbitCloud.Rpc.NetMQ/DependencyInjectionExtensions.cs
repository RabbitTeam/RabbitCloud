using Microsoft.Extensions.DependencyInjection;
using RabbitCloud.Config.Abstractions.Adapter;
using RabbitCloud.Rpc.NetMQ.Config;
using RabbitCloud.Rpc.NetMQ.Internal;

namespace RabbitCloud.Rpc.NetMQ
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddNetMqProtocol(this IServiceCollection services)
        {
            services
                .AddSingleton<IRouterSocketFactory, RouterSocketFactory>()
                .AddSingleton(new NetMqPollerHolder())
                .AddSingleton<NetMqProtocol, NetMqProtocol>()
                .AddSingleton<IProtocolProvider, NetMqProtocolProvider>();

            return services;
        }
    }
}