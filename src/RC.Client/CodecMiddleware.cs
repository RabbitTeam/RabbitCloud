using Microsoft.Extensions.Options;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;
using Rabbit.Cloud.Client.Features;
using Rabbit.Cloud.Client.Internal.Codec;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client
{
    public class CodecMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly RabbitClientOptions _clientOptions;

        public CodecMiddleware(RabbitRequestDelegate next, IOptions<RabbitClientOptions> clientOptions)
        {
            _next = next;
            _clientOptions = clientOptions.Value;
        }

        private ICodecFeature EnsureCodecFeature(IRabbitContext rabbitContext)
        {
            var rabbitClientFeature = rabbitContext.Features.Get<IRabbitClientFeature>();
            var codecFeature = rabbitContext.Features.Get<ICodecFeature>();

            if (codecFeature != null)
                return codecFeature;

            rabbitContext.Features.Set(codecFeature = new CodecFeature());

            var requestOptions = rabbitClientFeature.RequestOptions;

            var serializer = _clientOptions.SerializerTable.Get(requestOptions.SerializerName) ??
                _clientOptions.SerializerTable.Get("json");

            var codec = new SerializerCodec(serializer, rabbitClientFeature.RequestType, rabbitClientFeature.ResponseType);

            codecFeature.Codec = codec;

            return codecFeature;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var codecFeature = EnsureCodecFeature(context);

            var codec = codecFeature.Codec;

            if (codec == null)
            {
                await _next(context);
            }
            else
            {
                var rabbitClientFeature = context.Features.Get<IRabbitClientFeature>();
                var requestType = rabbitClientFeature.RequestType;
                var responseType = rabbitClientFeature.ResponseType;
                try
                {
                    if (requestType != null)
                        context.Request.Body = codec.Encode(context.Request.Body);
                    await _next(context);
                }
                finally
                {
                    if (responseType != null)
                        context.Response.Body = codec.Decode(context.Response.Body);
                }
            }
        }
    }
}