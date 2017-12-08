using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Abstractions.Extensions;

namespace Rabbit.Cloud.Client.LoadBalance.Builder
{
    public static class LoadBalanceApplicationBuilderExtensions
    {
        public static IRabbitApplicationBuilder UseLoadBalance(this IRabbitApplicationBuilder app)
        {
            return app
                .UseMiddleware<LoadBalanceConfigurationMiddleware>()
                .UseMiddleware<LoadBalanceMiddleware>();
        }
    }
}