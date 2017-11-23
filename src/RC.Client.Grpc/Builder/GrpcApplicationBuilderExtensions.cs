using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Abstractions.Extensions;

namespace Rabbit.Cloud.Client.Grpc.Builder
{
    public static class GrpcApplicationBuilderExtensions
    {
        public static IRabbitApplicationBuilder UseGrpc(this IRabbitApplicationBuilder app)
        {
            return app
                .UseMiddleware<GrpcMiddleware>();
        }
    }
}