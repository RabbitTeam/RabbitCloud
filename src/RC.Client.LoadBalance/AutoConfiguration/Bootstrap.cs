using Microsoft.Extensions.Hosting;

namespace Rabbit.Cloud.Client.LoadBalance.AutoConfiguration
{
    public static class Bootstrap
    {
        public static void Start(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((ctx, services) =>
            {
                services.AddLoadBalance(options =>
                {
                    foreach (var section in ctx.Configuration.GetSection("RabbitCloud:Client").GetChildren())
                    {
                        options.ServiceRequestOptions[section.Key] = LoadBalanceClientOptions.RequestOptions.Create(section, options);
                    }
                });
            });
        }
    }
}