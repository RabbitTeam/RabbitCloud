using Microsoft.Extensions.DependencyInjection;
using Rabbit.Rpc.ProxyGenerator.Implementation;
using Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;

namespace Rabbit.Rpc.ProxyGenerator
{
    public static class RpcServiceCollectionExtensions
    {
        public static IRpcBuilder AddClient(this IServiceCollection services)
        {
            services.AddSingleton<IServiceProxyGenerater, ServiceProxyGenerater>();
            services.AddSingleton<IServiceProxyFactory, ServiceProxyFactory>();

            return services
                .AddRpcCore()
                .AddClientCore()
                .SetAddressSelector<PollingAddressSelector>();
        }
    }
}