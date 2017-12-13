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

            var serverCallContext = requestFeature.ServerCallContext;
            requestFeature.ServiceUrl = new ServiceUrl
            {
                Host = serverCallContext.Host,
                Path = serverCallContext.Method,
                Scheme = "grpc"
            };

            var responseFeature = context.Features.Get<IGrpcServerResponseFeature>();
            responseFeature.Response = await responseFeature.GetResponseAsync();

            await _next(context);
        }
    }
}