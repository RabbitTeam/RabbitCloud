using Microsoft.Extensions.DependencyInjection;
using Rabbit.Rpc;
using Rabbit.Rpc.Runtime.Server;
using Rabbit.Rpc.Transport;

namespace Rabbit.Transport.DotNetty
{
    public static class RpcServiceCollectionExtensions
    {
        public static IRpcBuilder AddDotNettyTransport(this IRpcBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<ITransportClientFactory, DotNettyTransportClientFactory>();
            services.AddSingleton<IServiceHost, DotNettyServiceHost>();

            return builder;
        }

        public static IRpcBuilder AddServer(this IServiceCollection services)
        {
            services.AddSingleton<IServiceHost, DotNettyServiceHost>();

            return services
                .AddRpcCore()
                .AddServerCore();
        }
    }
}