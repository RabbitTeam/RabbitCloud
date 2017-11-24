using App.Metrics;
using App.Metrics.Apdex;
using App.Metrics.AspNetCore.Tracking;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Server.Monitor.Internal;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Server.Monitor.Middleware
{
    public class ApdexMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly IApdex _apdexTracking;

        public ApdexMiddleware(RabbitRequestDelegate next, IOptions<MetricsWebTrackingOptions> trackingMiddlwareOptionsAccessor, IMetrics metrics)
        {
            _next = next;
            _apdexTracking = metrics.Provider
                .Apdex
                .Instance(MetricsRegistry.ApdexScores.Apdex(trackingMiddlwareOptionsAccessor.Value.ApdexTSeconds));
        }

        public async Task Invoke(IRabbitContext context)
        {
            var apdex = _apdexTracking.NewContext();

            await _next(context);

            using (apdex)
            {
            }
        }
    }
}