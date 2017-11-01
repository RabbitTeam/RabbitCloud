using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Extensions;

namespace RC.Client.LoadBalance.Builder
{
    public static class LoadBalanceApplicationBuilderExtensions
    {
        public static IRabbitApplicationBuilder UseLoadBalance(this IRabbitApplicationBuilder app)
        {
            return app
                .UseMiddleware<LoadBalanceMiddleware>();
        }
    }
}