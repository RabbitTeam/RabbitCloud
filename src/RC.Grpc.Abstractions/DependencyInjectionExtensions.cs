using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Grpc.Abstractions.Internal;

namespace Rabbit.Cloud.Grpc.Abstractions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddGrpcCore(this IServiceCollection services)
        {
            return services
                .AddSingleton<IMethodTableProvider, DefaultMethodTableProvider>();
        }
    }
}