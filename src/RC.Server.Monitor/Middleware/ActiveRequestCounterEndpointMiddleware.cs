using App.Metrics;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Server.Monitor.Internal.Extensions;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Server.Monitor.Middleware
{
    public class ActiveRequestCounterEndpointMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly IMetrics _metrics;

        public ActiveRequestCounterEndpointMiddleware(RabbitRequestDelegate next, IMetrics metrics)
        {
            _next = next;
            _metrics = metrics;
        }

        public async Task Invoke(IRabbitContext context)
        {
            _metrics.IncrementActiveRequests();
            try
            {
                await _next(context);
            }
            finally
            {
                _metrics.DecrementActiveRequests();
            }
        }
    }
}