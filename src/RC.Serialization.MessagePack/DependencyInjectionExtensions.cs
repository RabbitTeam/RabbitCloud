using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Abstractions.Serialization;

namespace Rabbit.Cloud.Serialization.MessagePack
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddMessagePackSerializer(this IServiceCollection services)
        {
            return services.AddSingleton<ISerializer, MessagePackSerializer>();
        }
    }
}