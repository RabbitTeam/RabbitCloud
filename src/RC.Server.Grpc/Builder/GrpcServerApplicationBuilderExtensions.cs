using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Abstractions.Extensions;

namespace Rabbit.Cloud.Server.Grpc.Builder
{
    public static class GrpcServerApplicationBuilderExtensions
    {
        public static IRabbitApplicationBuilder UseServerGrpc(this IRabbitApplicationBuilder app)
        {
            return app.UseMiddleware<GrpcServerMiddleware>();
        }
    }
}