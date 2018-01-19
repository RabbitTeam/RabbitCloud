using Rabbit.Cloud.Application.Abstractions.Extensions;
using Rabbit.Cloud.Client;

// ReSharper disable once CheckNamespace
namespace Rabbit.Cloud.Application.Abstractions
{
    public static class ClientBuilderExtensions
    {
        public static IRabbitApplicationBuilder UseRabbitClient(this IRabbitApplicationBuilder app)
        {
            return app
                .UseMiddleware<ServiceDiscoveryMiddleware>()
                .UseMiddleware<RequestOptionMiddleware>()
                .UseMiddleware<ClientMiddleware>()
                .UseMiddleware<LoadBalanceMiddleware>();
        }

        public static IRabbitApplicationBuilder UseCodec(this IRabbitApplicationBuilder app)
        {
            return app
                .UseMiddleware<CodecMiddleware>();
        }
    }
}