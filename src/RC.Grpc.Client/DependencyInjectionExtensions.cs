using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Grpc.Abstractions;
using Rabbit.Cloud.Grpc.Abstractions.Internal;
using Rabbit.Cloud.Grpc.Client.Internal;
using System;

namespace Rabbit.Cloud.Grpc.Client
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddGrpcClient(this IServiceCollection services, Action<MethodProviderOptions> configure)
        {
            return services
                .AddGrpcCore(configure)
                .AddGrpcClient();
        }

        public static IServiceCollection AddGrpcClient(this IServiceCollection services)
        {
            return services
                .AddSingleton<ChannelPool>()
                .AddSingleton<CallInvokerPool>();
        }
    }
}