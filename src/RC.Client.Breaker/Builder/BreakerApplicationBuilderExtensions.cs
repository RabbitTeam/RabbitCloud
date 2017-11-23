using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Abstractions.Extensions;

namespace Rabbit.Cloud.Client.Breaker.Builder
{
    public static class BreakerApplicationBuilderExtensions
    {
        public static IRabbitApplicationBuilder UseBreaker(this IRabbitApplicationBuilder app)
        {
            return app
                .UseMiddleware<BreakerMiddleware>();
        }

        public static IRabbitApplicationBuilder UseBreaker(this IRabbitApplicationBuilder app, BreakerOptions options)
        {
            return app
                .UseMiddleware<BreakerMiddleware>(options);
        }
    }
}