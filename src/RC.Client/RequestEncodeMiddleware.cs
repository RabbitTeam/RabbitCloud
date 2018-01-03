using Microsoft.Extensions.Logging;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Features;
using System.IO;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client
{
    public class ResposneDecodeMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly ILogger<ResposneDecodeMiddleware> _logger;

        public ResposneDecodeMiddleware(RabbitRequestDelegate next, ILogger<ResposneDecodeMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var codecFeature = context.Features.Get<ICodecFeature>();
            if (codecFeature?.Codec != null && context.Response.Body != null)
                context.Response.Body = codecFeature.Codec.Decode((Stream)context.Response.Body);

            await _next(context);
        }
    }
}