using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Abstractions.Extensions;
using Rabbit.Cloud.Server.Monitor.Middleware;

namespace Rabbit.Cloud.Server.Monitor.Builder
{
    public static class MonitorApplicationBuilderExtensions
    {
        public static IRabbitApplicationBuilder UseAllMonitor(this IRabbitApplicationBuilder app)
        {
            return app
                .UseMiddleware<ActiveRequestCounterEndpointMiddleware>()
                .UseMiddleware<ErrorRequestMeterMiddleware>()
                .UseMiddleware<RequestTimerMiddleware>()
                .UseMiddleware<PerRequestTimerMiddleware>()
                .UseMiddleware<ApdexMiddleware>();
        }
    }
}