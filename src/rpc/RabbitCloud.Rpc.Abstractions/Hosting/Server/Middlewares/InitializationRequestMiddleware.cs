using RabbitCloud.Rpc.Abstractions.Features;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Hosting.Server.Middlewares
{
    public class InitializationRequestMiddleware
    {
        private readonly RpcRequestDelegate _next;

        public InitializationRequestMiddleware(RpcRequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(RpcContext context)
        {
            var codec = context.Features.Get<ICodecFeature>().Codec;

            var requestFeature = context.Features.Get<IRpcRequestFeature>();

            var invocation = (IRpcRequestFeature)codec.Decode(context.Request.Body, typeof(IRpcRequestFeature));
            requestFeature.ServiceId = invocation.ServiceId;
            requestFeature.Body = invocation.Body;

            await _next(context);
        }
    }
}