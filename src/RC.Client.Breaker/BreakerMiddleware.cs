using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Breaker.Features;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Breaker
{
    public class BreakerMiddleware
    {
        private readonly RabbitRequestDelegate _next;

        public BreakerMiddleware(RabbitRequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var breakerFeature = context.Features.Get<IBreakerFeature>();
            var policy = breakerFeature?.Policy;
            if (policy == null)
            {
                await _next(context);
                return;
            }

            await policy.ExecuteAsync(() => _next(context));
        }
    }
}