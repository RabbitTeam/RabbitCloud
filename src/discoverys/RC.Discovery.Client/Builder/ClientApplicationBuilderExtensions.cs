using Rabbit.Cloud.Discovery.Client.Middlewares;
using RC.Discovery.Client.Abstractions;
using RC.Discovery.Client.Abstractions.Extensions;

namespace Rabbit.Cloud.Discovery.Client.Builder
{
    public static class ClientApplicationBuilderExtensions
    {
        public static IRabbitApplicationBuilder UseRabbitClient(this IRabbitApplicationBuilder app)
        {
            return app.UseMiddleware<ServiceAddressResolveMiddleware>()
                .UseMiddleware<ServiceRequestMiddleware>();
        }
    }
}