using Rabbit.Cloud.Abstractions;
using Rabbit.Cloud.Abstractions.Extensions;
using Rabbit.Cloud.Cluster.HighAvailability;
using Rabbit.Cloud.Cluster.LoadBalance;

namespace Rabbit.Cloud.Cluster
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