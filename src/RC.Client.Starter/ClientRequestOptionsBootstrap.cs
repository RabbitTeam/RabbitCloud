using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Abstractions.Client;
using Rabbit.Cloud.Abstractions.Utilities;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Application.Abstractions.Extensions;
using Rabbit.Cloud.Application.Features;
using System;
using Rabbit.Cloud.Client.LoadBalance.Builder;

namespace Rabbit.Cloud.Client.Starter
{
    public static class ClientRequestOptionsBootstrap
    {
        public static int Priority => 50;

        public static void Start(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((ctx, services) =>
            {
                services.Configure<ClientRequestOptions>(options =>
                {
                    foreach (var section in ctx.Configuration.GetSection("RabbitCloud:Client").GetChildren())
                    {
                        options.Configurations[section.Key] = section;
                    }
                });
            }).ConfigureRabbitApplication((ctx, serviceCollection, services, app) =>
            {
                app
                .Use(async (c, next) =>
                {
                    var clientRequestOptions = services.GetRequiredService<IOptions<ClientRequestOptions>>().Value;

                    if (clientRequestOptions.Configurations.TryGetValue(c.Request.Url.Host, out var serviceConfiguration))
                    {
                        var requestFeature = c.Features.GetOrAdd<IRequestFeature>(() => new RequestFeature());
                        requestFeature.ConnectionTimeout = TimeUtilities.GetTimeSpanBySimpleOrDefault(serviceConfiguration[nameof(IRequestFeature.ConnectionTimeout)], TimeSpan.FromSeconds(2));
                        requestFeature.ReadTimeout = TimeUtilities.GetTimeSpanBySimpleOrDefault(serviceConfiguration[nameof(IRequestFeature.ReadTimeout)], TimeSpan.FromSeconds(10));
                    }

                    await next();
                })
                    .UseLoadBalance();
            });
        }
    }
}