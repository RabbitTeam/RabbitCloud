using Microsoft.Extensions.DependencyInjection;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Proxy;
using RabbitCloud.Rpc.Proxy;

namespace RabbitCloud.Rpc
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddRabbitRpc(this IServiceCollection services)
        {
            return services
                .AddSingleton<IRequestIdGenerator, DefaultRequestIdGenerator>()
                .AddSingleton<IProxyFactory, ProxyFactory>();
        }
    }
}