using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Grpc.Abstractions.Internal;
using Rabbit.Cloud.Grpc.Abstractions.Method;
using System;

namespace Rabbit.Cloud.Grpc.Abstractions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddGrpcCore(this IServiceCollection services, Action<DefaultMethodProviderOptions> configure)
        {
            return services
                .Configure(configure)
                .AddSingleton<IMethodCollection, MethodCollection>()
                .AddSingleton<IMethodProvider, DefaultMethodProvider>();
        }
    }
}