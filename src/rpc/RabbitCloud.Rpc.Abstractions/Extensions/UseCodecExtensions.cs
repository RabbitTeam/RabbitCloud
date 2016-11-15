using RabbitCloud.Rpc.Abstractions.Features;

namespace RabbitCloud.Rpc.Abstractions.Extensions
{
    public static class UseCodecExtensions
    {
        public static IRpcApplicationBuilder UseCodec(this IRpcApplicationBuilder app, ICodec codec)
        {
            return app.Use(async (context, next) =>
            {
                context.Features.Set<ICodecFeature>(new CodecFeature
                {
                    Codec = codec
                });
                await next();
            });
        }
    }
}