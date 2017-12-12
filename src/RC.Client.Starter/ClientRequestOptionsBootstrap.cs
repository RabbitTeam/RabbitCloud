using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Rabbit.Cloud.Abstractions.Utilities;
using Rabbit.Cloud.Application.Abstractions.Extensions;
using Rabbit.Cloud.Application.Features;
using Rabbit.Cloud.Client.LoadBalance.Builder;
using Rabbit.Cloud.Hosting;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Starter
{
    public static class ClientRequestOptionsBootstrap
    {
        private class RequestOptions
        {
            public TimeSpan ConnectionTimeout { get; set; }
            public TimeSpan ReadTimeout { get; set; }

            public static RequestOptions Default { get; } = new RequestOptions
            {
                ConnectionTimeout = TimeSpan.FromSeconds(2),
                ReadTimeout = TimeSpan.FromSeconds(10)
            };

            public void Apply(IRequestFeature requestFeature)
            {
                requestFeature.ConnectionTimeout = ConnectionTimeout;
                requestFeature.ReadTimeout = ReadTimeout;
            }

            public static RequestOptions Create(IConfiguration configuration)
            {
                return new RequestOptions
                {
                    ConnectionTimeout =
                        TimeUtilities.GetTimeSpanBySimpleOrDefault(configuration[nameof(ConnectionTimeout)],
                            Default.ConnectionTimeout),
                    ReadTimeout =
                        TimeUtilities.GetTimeSpanBySimpleOrDefault(configuration[nameof(ReadTimeout)],
                            Default.ReadTimeout)
                };
            }
        }

        private static readonly IDictionary<string, RequestOptions> RequestOptionses =
            new Dictionary<string, RequestOptions>(StringComparer.OrdinalIgnoreCase);

        public static int Priority => 50;

        public static void Start(IHostBuilder hostBuilder)
        {
            hostBuilder
                .ConfigureServices((ctx, services) =>
                {
                    foreach (var section in ctx.Configuration.GetSection("RabbitCloud:Client").GetChildren())
                    {
                        RequestOptionses[section.Key] = RequestOptions.Create(section);
                    }
                })
                .ConfigureRabbitApplication((ctx, app) =>
                {
                    app
                        .Use(async (c, next) =>
                        {
                            if (RequestOptionses.TryGetValue(c.Request.Url.Host, out var options))
                            {
                                var requestFeature = c.Features.GetOrAdd<IRequestFeature>(() => new RequestFeature());
                                options.Apply(requestFeature);
                            }

                            await next();
                        })
                        .UseLoadBalance();
                });
        }
    }
}