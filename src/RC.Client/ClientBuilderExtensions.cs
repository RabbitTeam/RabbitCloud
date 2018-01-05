using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Abstractions.Extensions;

namespace Rabbit.Cloud.Client
{
    public static class ClientBuilderExtensions
    {
        public static IRabbitApplicationBuilder UseRabbitClient(this IRabbitApplicationBuilder app)
        {
            return app.UseMiddleware<RequestOptionMiddleware>()
                .UseMiddleware<ServiceInstanceMiddleware>()
                .UseMiddleware<ClientMiddleware>();
        }
    }
}