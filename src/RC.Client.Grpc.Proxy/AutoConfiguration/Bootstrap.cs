using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Proxy;

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
                            new GrpcProxyInterceptor(p.GetRequiredService<RabbitRequestDelegate>(),
                                p.GetRequiredService<IOptions<RabbitCloudOptions>>())
                        }))
                        .AddSingleton<IInterceptor, GrpcProxyInterceptor>()
                        .AddSingleton<IProxyFactory, ProxyFactory>();
                });
        }
    }
}