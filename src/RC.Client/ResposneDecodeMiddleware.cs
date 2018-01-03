using Microsoft.Extensions.Logging;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Features;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client
{
    public class RequestEncodeMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly ILogger<RequestEncodeMiddleware> _logger;

        public RequestEncodeMiddleware(RabbitRequestDelegate next, ILogger<RequestEncodeMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var codecFeature = context.Features.Get<ICodecFeature>();
            if (codecFeature?.Codec != null && context.Request.Body != null)
                context.Request.Body = codecFeature.Codec.Encode(context.Request.Body);

            await _next(context);
        }
    }
}