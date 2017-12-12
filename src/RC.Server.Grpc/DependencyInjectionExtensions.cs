using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Grpc.ApplicationModels;
using Rabbit.Cloud.Server.Grpc.Internal;

namespace Rabbit.Cloud.Server.Grpc
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddServerGrpc(this IServiceCollection services)
        {
            return services
                .InternalAddServerGrpc();
        }

        private static IServiceCollection InternalAddServerGrpc(this IServiceCollection services)
        {
            return services
                .AddSingleton<IServerMethodInvokerFactory, ServerMethodInvokerFactory>();
        }
    }
}