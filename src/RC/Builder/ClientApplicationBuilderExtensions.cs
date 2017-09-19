using Rabbit.Cloud.Middlewares;
using RC.Abstractions;
using RC.Discovery.Client.Abstractions.Extensions;

namespace Rabbit.Cloud.Builder
{
    public static class ClientApplicationBuilderExtensions
    {
        public static IRabbitApplicationBuilder UseServiceContainer(this IRabbitApplicationBuilder app)
        {
            return app
                .UseMiddleware<ServiceContainerMiddleware>();
        }

        public static IRabbitApplicationBuilder UseRabbitClient(this IRabbitApplicationBuilder app)
        {
            return app
                .UseMiddleware<ServiceRequestMiddleware>();
        }
    }
}