using Microsoft.Extensions.DependencyInjection;
using Rabbit.Rpc;
using Rabbit.Rpc.Runtime.Server;
using Rabbit.Rpc.Transport;

namespace Rabbit.Transport.DotNetty
{
    public static class RpcServiceCollectionExtensions
    {
        public static IRpcBuilder AddDotNettyClient(this IRpcBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<ITransportClientFactory, DotNettyTransportClientFactory>();

            return builder;
        }

        public static IRpcBuilder AddDotNettyServer(this IRpcBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<IServiceHost, DotNettyServiceHost>();

            return builder;
        }
    }
}