using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Features;
using RabbitCloud.Rpc.Default.Features;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Default.Middlewares
{
    public class ResponseMiddleware
    {
        private readonly RpcRequestDelegate _next;

        public ResponseMiddleware(RpcRequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(RpcContext context)
        {
            var features = context.Features;
            var session = features.Get<ISessionFeature>().Session;
            var codec = features.Get<ICodecFeature>().Codec;

            var data = (byte[])codec.Encode(features.Get<IRpcResponseFeature>());

            await session.SendAsync(data);
            await _next(context);
        }
    }
}