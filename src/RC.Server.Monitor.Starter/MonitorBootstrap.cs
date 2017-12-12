using App.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Server.Monitor.Builder;
using System.Collections.Generic;

namespace Rabbit.Cloud.Server.Monitor.Starter
{
    public class MonitorOptions
    {
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; }
        public string Server { get; set; }
        public ReportOptions Report { get; set; }

        public class ReportOptions
        {
            public string Type { get; set; }
            public IDictionary<string, string> Attributes { get; set; }
        }
    }

    public class MonitorBootstrap
    {
        public static int Priority => 20;

        public static void Start(IHostBuilder hostBuilder)
        {
            hostBuilder
                .ConfigureServices((ctx, services) =>
                {
                    var monitorOptions = ctx.Configuration.GetSection("RabbitCloud:Monitor").Get<MonitorOptions>();

                    if (string.IsNullOrEmpty(monitorOptions.ApplicationName))
                        monitorOptions.ApplicationName = ctx.HostingEnvironment.ApplicationName;
                    if (string.IsNullOrEmpty(monitorOptions.EnvironmentName))
                        monitorOptions.EnvironmentName = ctx.HostingEnvironment.EnvironmentName;

                    var builder = new MetricsBuilder()
                        .Configuration
                        .Configure(options =>
                        {
                            options
                                .AddEnvTag(monitorOptions.EnvironmentName)
                                .AddAppTag(monitorOptions.ApplicationName)
                                .AddServerTag(monitorOptions.Server);
                        });

                    switch (monitorOptions.Report?.Type?.ToLower())
                    {
                        case "elasticsearch":
                            builder.Report.ToElasticsearch(monitorOptions.Report.Attributes["Url"], monitorOptions.Report.Attributes["Index"]);
                            break;
                    }

                    services
                    .AddSingleton<IMetrics>(builder.Build())
                    .AddSingleton<IHostedService, ReportRunnerService>();
                })
                .ConfigureRabbitApplication((ctx, services, app) =>
                {
                    app.UseAllMonitor();
                });
        }
    }
}