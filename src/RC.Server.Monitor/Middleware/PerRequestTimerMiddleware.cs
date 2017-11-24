using App.Metrics;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Server.Monitor.Internal.Extensions;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Server.Monitor.Middleware
{
    public class PerRequestTimerMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly IMetrics _metrics;

        public PerRequestTimerMiddleware(RabbitRequestDelegate next, IMetrics metrics)
        {
            _next = next;
            _metrics = metrics;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var startTime = _metrics.Clock.Nanoseconds;
            await _next(context);
            var elapsed = _metrics.Clock.Nanoseconds - startTime;
            _metrics.RecordEndpointsRequestTime(null, context.GetServiceId(), elapsed);
        }
    }
}