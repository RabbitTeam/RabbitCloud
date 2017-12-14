using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Proxy;
using Rabbit.Cloud.Grpc.ApplicationModels.Internal;

namespace Rabbit.Cloud.Client.Grpc.Proxy.AutoConfiguration
{
    public class Bootstrap
    {
        public static void Start(IHostBuilder hostBuilder)
        {
            hostBuilder
                .ConfigureServices(services =>
                {
                    services
                        .AddSingleton<IProxyFactory>(p => new ProxyFactory(new[]
                        {
                            new GrpcProxyInterceptor(p.GetRequiredService<RabbitRequestDelegate>(),p.GetRequiredService<SerializerCacheTable>())
                        }))
                        .AddSingleton<IInterceptor, GrpcProxyInterceptor>()
                        .AddSingleton<IProxyFactory, ProxyFactory>();
                });
        }
    }
}