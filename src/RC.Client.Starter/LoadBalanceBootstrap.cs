using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rabbit.Cloud.Client.LoadBalance;
using System;
using System.Linq;

namespace Rabbit.Cloud.Client.Starter
{
    public static class LoadBalanceBootstrap
    {
        public static void Start(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((ctx, services) =>
            {
                services
                .AddLoadBalance()
                .Configure<LoadBalanceConfigurationOptions>(options =>
                {
                    var loadBalanceConfiguration = ctx.Configuration.GetSection("RabbitCloud:Client");

                    var loadBalanceConfigurationItems = loadBalanceConfiguration.GetChildren().ToDictionary(i => i.Key,
                        s => s.Get<LoadBalanceConfigurationOptions.LoadBalanceConfigurationItem>(),
                        StringComparer.OrdinalIgnoreCase);

                    foreach (var loadBalanceConfigurationItem in loadBalanceConfigurationItems)
                    {
                        options.ConfigurationItems.Add(loadBalanceConfigurationItem);
                    }
                });
            });
        }
    }
}