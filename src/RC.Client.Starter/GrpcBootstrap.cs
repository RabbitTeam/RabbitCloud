using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Grpc.Builder;
using Rabbit.Cloud.Client.Grpc.Proxy;
using Rabbit.Cloud.Client.Proxy;
using Rabbit.Cloud.Grpc;
using Rabbit.Cloud.Hosting;

namespace Rabbit.Cloud.Client.Starter
{
    public class GrpcBootstrap
    {
        public static int Priority => 10;

        public static void Start(IHostBuilder hostBuilder)
        {
            hostBuilder
                .ConfigureServices(services =>
                {
                    services.AddGrpcClient();

                    services
                        .AddSingleton<IProxyFactory>(p => new ProxyFactory(new[]
                        {
                            new GrpcProxyInterceptor(p.GetRequiredService<RabbitRequestDelegate>(),
                                p.GetRequiredService<IOptions<RabbitCloudOptions>>())
                        }))
                        .AddSingleton<IInterceptor, GrpcProxyInterceptor>()
                        .AddSingleton<IProxyFactory, ProxyFactory>();
                })
                .ConfigureRabbitApplication(app =>
                {
                    app.UseGrpc();
                });
        }
    }
}