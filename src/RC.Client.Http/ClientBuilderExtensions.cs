using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Abstractions.Extensions;

namespace Rabbit.Cloud.Client.Http
{
    public static class ClientBuilderExtensions
    {
        public static IRabbitApplicationBuilder UseRabbitHttpClient(this IRabbitApplicationBuilder app)
        {
            return app
                .MapWhen<IRabbitContext>(
                    c => string.Equals(c.Request.Scheme, "http") || string.Equals(c.Request.Scheme, "https"),
                    appBuilder =>
                    {
                        appBuilder.UseMiddleware<HttpMiddleware>();
                    });
        }
    }
}