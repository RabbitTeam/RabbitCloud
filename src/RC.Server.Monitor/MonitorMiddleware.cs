using App.Metrics;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Application.Abstractions;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Monitor
{
    public class MonitorMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly IMetrics _metrics;

        public MonitorMiddleware(RabbitRequestDelegate next, IOptions<MonitorOptions> options)
        {
            _next = next;
            _metrics = options.Value.Metrics;
        }

        public async Task Invoke(IRabbitContext context)
        {
            _metrics.Measure.Counter.Increment(MetricsDefinition.TotalCount);
            using (_metrics.Measure.Timer.Time(context.GetServiceRequestTimer()))
            {
                try
                {
                    await _next(context);
                }
                catch
                {
                    _metrics.Measure.Counter.Increment(context.GetServiceErrors());
                    throw;
                }
            }
        }
    }
}