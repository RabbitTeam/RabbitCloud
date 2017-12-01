using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Grpc.Fluent.ApplicationModels;
using Rabbit.Cloud.Server.Grpc.Builder;
using Rabbit.Cloud.Server.Grpc.Internal;
using System;

namespace Rabbit.Cloud.Server.Grpc
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddServerGrpc(this IServiceCollection services)
        {
            return services
                .AddServerGrpc(s => s.AddOptions().BuildServiceProvider(),
                app =>
                {
                    app.UseServerGrpc();
                });
        }

        public static IServiceCollection AddServerGrpc(this IServiceCollection services, Func<IServiceCollection, IServiceProvider> servicesFactory, Action<IRabbitApplicationBuilder> application)
        {
            return services
                .AddServerGrpc(options =>
                {
                    var serverServices = servicesFactory(new ServiceCollection());

                    var serverApp = new RabbitApplicationBuilder(serverServices);

                    application(serverApp);

                    options.Invoker = serverApp.Build();
                });
        }

        public static IServiceCollection AddServerGrpc(this IServiceCollection services, Action<GrpcServerOptions> configure)
        {
            return services
                .InternalAddServerGrpc()
                .Configure(configure);
        }

        private static IServiceCollection InternalAddServerGrpc(this IServiceCollection services)
        {
            return services
                .AddSingleton<IServerMethodInvokerFactory, ServerMethodInvokerFactory>();
        }
    }
}