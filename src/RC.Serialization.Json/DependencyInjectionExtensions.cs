using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Abstractions.Serialization;

namespace Rabbit.Cloud.Serialization.Json
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddJsonSerializer(this IServiceCollection services)
        {
            return services.AddSingleton<ISerializer, JsonSerializer>();
        }
    }
}