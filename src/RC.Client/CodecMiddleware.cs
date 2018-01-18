using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client
{
    public class CodecMiddleware
    {
        private readonly RabbitRequestDelegate _next;

        public CodecMiddleware(RabbitRequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var codecFeature = context.Features.Get<ICodecFeature>();
            var codec = codecFeature?.Codec;

            if (codec == null)
            {
                await _next(context);
            }
            else
            {
                try
                {
                    context.Request.Body = codec.Encode(context.Request.Body);
                    await _next(context);
                }
                finally
                {
                    context.Response.Body = codec.Decode(context.Response.Body);
                }
            }
        }
    }
}