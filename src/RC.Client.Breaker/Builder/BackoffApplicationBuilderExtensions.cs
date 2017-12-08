/*using Microsoft.Extensions.Options;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Abstractions.Extensions;

namespace Rabbit.Cloud.Client.Breaker.Builder
{
    public static class BackoffApplicationBuilderExtensions
    {
        public static IRabbitApplicationBuilder UseBackoff(this IRabbitApplicationBuilder app, BackoffOptions options)
        {
            return app
                .UseMiddleware<BackoffMiddleware>(Options.Create(options));
        }

        public static IRabbitApplicationBuilder UseBackoff(this IRabbitApplicationBuilder app)
        {
            return app
                .UseMiddleware<BackoffMiddleware>();
        }
    }
}*/