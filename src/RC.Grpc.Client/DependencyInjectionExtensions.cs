using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Grpc.Client.Internal;

namespace Rabbit.Cloud.Grpc.Client
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddGrpcClient(this IServiceCollection services)
        {
            return services
                .AddSingleton<ChannelPool>()
                .AddSingleton<CallInvokerPool>();
        }
    }
}