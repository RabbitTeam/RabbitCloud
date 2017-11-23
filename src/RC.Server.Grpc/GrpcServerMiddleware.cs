using Rabbit.Cloud.Application.Abstractions;
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
            var serverContext = (GrpcServerRabbitContext)context;

            serverContext.Response.Response = await serverContext.LogicInvoker();

            await _next(context);
        }
    }
}