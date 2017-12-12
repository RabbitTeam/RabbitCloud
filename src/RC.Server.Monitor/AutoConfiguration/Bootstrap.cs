using App.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Server.Monitor.AutoConfiguration
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
            public IDictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public class Bootstrap
    {
        public static int Priority => 20;

        public static void Start(IHostBuilder hostBuilder)
        {
            hostBuilder
                .ConfigureServices((ctx, services) =>
                {
                    var monitorConfiguration = ctx.Configuration.GetSection("RabbitCloud:Monitor");

                    var monitorOptions = monitorConfiguration.Get<MonitorOptions>();
                    var reportOptions = monitorOptions.Report;
                    foreach (var section in monitorConfiguration.GetSection("Report").GetChildren())
                    {
                        switch (section.Key)
                        {
                            case "Type":
                                reportOptions.Type = section.Value;
                                break;

                            default:
                                reportOptions.Attributes[section.Key] = section.Value;
                                break;
                        }
                    }

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
                            builder.Report.ToElasticsearch(monitorOptions.Report.Attributes["Url"],
                                monitorOptions.Report.Attributes["Index"]);
                            break;
                    }

                    services
                        .AddSingleton<IMetrics>(builder.Build())
                        .AddSingleton<IHostedService, ReportRunnerService>();
                });
        }
    }
}