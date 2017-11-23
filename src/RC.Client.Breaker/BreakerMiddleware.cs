using Microsoft.Extensions.Options;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Breaker.Features;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Breaker
{
    public class BreakerMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly BreakerOptions _breakerOptions;

        public BreakerMiddleware(RabbitRequestDelegate next, IOptions<BreakerOptions> breakerOptions) : this(next, breakerOptions.Value)
        {
        }

        public BreakerMiddleware(RabbitRequestDelegate next, BreakerOptions breakerOptions)
        {
            _next = next;
            _breakerOptions = breakerOptions;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var strategy = GetStrategy(context);

            if (!_breakerOptions.Policies.TryGetValue(strategy, out var policy))
                policy = _breakerOptions.DefaultPolicy;

            if (policy == null)
            {
                await _next(context);
                return;
            }

            await policy.ExecuteAsync(() => _next(context));
        }

        private static string GetStrategy(IRabbitContext context)
        {
            var breakerFeature = context.Features.Get<IBreakerFeature>();
            return breakerFeature?.Strategy ?? context.Request.Url.Path;
        }
    }
}