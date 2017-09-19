using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Internal;
using RC.Abstractions;

namespace Rabbit.Cloud
{
    public static class DependencyInjectionExtensions
    {
        public static IRabbitBuilder AddRabbitCloudCore(this IServiceCollection services)
        {
            var builder = new RabbitBuilder(services);
            return builder;
        }
    }
}