using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Extensions;
using RabbitCloud.Rpc.Abstractions.Features;

namespace RabbitCloud.Rpc.Default.Extensions
{
    public static class CodecExtensions
    {
        public static IRpcApplicationBuilder UseCodec(this IRpcApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var codec = context.Features.Get<ICodecFeature>().Codec;

                var requestFeature = context.Features.Get<IRpcRequestFeature>();

                var invocation = (IRpcRequestFeature)codec.Decode(context.Request.Body, typeof(IRpcRequestFeature));
                requestFeature.ServiceId = invocation.ServiceId;
                requestFeature.Body = invocation.Body;

                await next.Invoke();
            });
        }
    }
}