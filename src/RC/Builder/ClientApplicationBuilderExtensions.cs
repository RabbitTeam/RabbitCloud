using Rabbit.Cloud.Abstractions;
using Rabbit.Cloud.Abstractions.Extensions;
using Rabbit.Cloud.Middlewares;

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