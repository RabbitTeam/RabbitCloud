using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Features;
using Rabbit.Cloud.Server.Grpc.Features;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Server.Grpc
{
    public class GrpcServerMiddleware
    {
        private readonly RabbitRequestDelegate _next;

        public GrpcServerMiddleware(RabbitRequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var requestFeature = context.Features.Get<IGrpcServerRequestFeature>();

            requestFeature.ServiceUrl = new ServiceUrl($"grpc://{requestFeature.ServerCallContext.Host}{requestFeature.ServerCallContext.Method}");

            var responseFeature = context.Features.Get<IGrpcServerResponseFeature>();
            responseFeature.Response = await responseFeature.GetResponseAsync();

            await _next(context);
        }
    }
}