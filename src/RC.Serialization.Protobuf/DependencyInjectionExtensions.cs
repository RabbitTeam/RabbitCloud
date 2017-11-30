using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Abstractions.Serialization;

namespace Rabbit.Cloud.Serialization.Protobuf
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddProtobufSerializer(this IServiceCollection services)
        {
            return services.AddSingleton<ISerializer, ProtobufSerializer>();
        }
    }
}