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
            var grpcServerFeature = context.Features.Get<IGrpcServerFeature>();

            var serverCallContext = grpcServerFeature.ServerCallContext;
            context.Request.Url = new ServiceUrl
            {
                Host = serverCallContext.Host,
                Path = serverCallContext.Method,
                Scheme = "grpc"
            };

            context.Response.Response = context.Response.Response ?? await grpcServerFeature.ResponseInvoker();

            await _next(context);
        }
    }
}