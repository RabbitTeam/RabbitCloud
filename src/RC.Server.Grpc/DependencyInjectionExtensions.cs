using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Grpc.Fluent.ApplicationModels;
using Rabbit.Cloud.Server.Grpc.Internal;
using System;

namespace Rabbit.Cloud.Server.Grpc
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddServerGrpc(this IServiceCollection services, Action<GrpcServerOptions> configure)
        {
            return services
                .Configure(configure)
                .AddSingleton<IServerMethodInvokerFactory, ServerMethodInvokerFactory>();
        }
    }
}