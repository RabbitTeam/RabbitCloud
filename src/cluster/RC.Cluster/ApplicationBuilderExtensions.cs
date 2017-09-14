using RC.Cluster.HighAvailability;
using RC.Cluster.LoadBalance;
using RC.Discovery.Client.Abstractions;
using RC.Discovery.Client.Abstractions.Extensions;

namespace RC.Cluster
{
    public static class ApplicationBuilderExtensions
    {
        public static IRabbitApplicationBuilder UseHighAvailability(this IRabbitApplicationBuilder app)
        {
            return app
                .UseMiddleware<HighAvailabilityMiddleware>();
        }

        public static IRabbitApplicationBuilder UseLoadBalance(this IRabbitApplicationBuilder app)
        {
            return app
                .UseMiddleware<LoadBalanceMiddleware>();
        }
    }
}