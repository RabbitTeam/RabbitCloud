using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Grpc.Server.Internal;

namespace Rabbit.Cloud.Grpc.Server
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddGrpcServer(this IServiceCollection services)
        {
            return services
                .AddSingleton<IServerServiceDefinitionTableProvider, ApplicationModelServerServiceDefinitionTableProvider>();
        }
    }
}