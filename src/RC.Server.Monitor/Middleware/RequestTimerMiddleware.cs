using App.Metrics;
using App.Metrics.Timer;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Server.Monitor.Internal;
using System;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Server.Monitor.Middleware
{
    public class RequestTimerMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly ITimer _requestTimer;

        public RequestTimerMiddleware(RabbitRequestDelegate next, IMetrics metrics)
        {
            _next = next;
            _requestTimer = metrics.Provider
                .Timer
                .Instance(MetricsRegistry.Timers.RequestTransactionDuration);
        }

        public async Task Invoke(IRabbitContext context)
        {
            await _next(context);
            using (_requestTimer as IDisposable)
            {
            }
        }
    }
}