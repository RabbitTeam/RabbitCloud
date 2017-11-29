using App.Metrics;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Server.Monitor.Internal.Extensions;
using System;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Server.Monitor.Middleware
{
    public class ErrorRequestMeterMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly IMetrics _metrics;

        public ErrorRequestMeterMiddleware(RabbitRequestDelegate next, IMetrics metrics)
        {
            _next = next;
            _metrics = metrics;
        }

        public async Task Invoke(IRabbitContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                var serviceId = context.GetServiceId();
                _metrics.RecordRequestError(serviceId, -1);
                _metrics.RecordException(serviceId, exception.GetType().FullName);
                throw;
            }
        }
    }
}