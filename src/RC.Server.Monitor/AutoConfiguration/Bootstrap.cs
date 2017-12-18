using App.Metrics;
using App.Metrics.Reporting.Elasticsearch;
using App.Metrics.Reporting.InfluxDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rabbit.Cloud.Abstractions.Utilities;
using System;
using System.Collections.Generic;
using System.Net;

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
                    var reportConfiguration = monitorConfiguration.GetSection("Report");

                    if (!monitorConfiguration.Exists() || !reportConfiguration.Exists())
                        return;

                    var reportType = reportConfiguration["Type"];

                    if (string.IsNullOrWhiteSpace(reportType))
                        return;

                    var monitorOptions = monitorConfiguration.Get<MonitorOptions>();

                    if (string.IsNullOrEmpty(monitorOptions.ApplicationName))
                        monitorOptions.ApplicationName = ctx.HostingEnvironment.ApplicationName;
                    if (string.IsNullOrEmpty(monitorOptions.EnvironmentName))
                        monitorOptions.EnvironmentName = ctx.HostingEnvironment.EnvironmentName;
                    if (string.IsNullOrWhiteSpace(monitorOptions.Server))
                        monitorOptions.Server = Dns.GetHostName();

                    var builder = new MetricsBuilder()
                        .Configuration
                        .Configure(options =>
                        {
                            options
                                .AddEnvTag(monitorOptions.EnvironmentName)
                                .AddAppTag(monitorOptions.ApplicationName)
                                .AddServerTag(monitorOptions.Server);
                        });

                    var url = reportConfiguration.GetValue<Uri>("Url");
                    var flushInterval = TimeUtilities.GetTimeSpanBySimpleOrDefault(reportConfiguration["FlushInterval"], TimeSpan.FromSeconds(10));
                    switch (reportType.ToLower())
                    {
                        case "elasticsearch":
                            builder.Report.ToElasticsearch(options =>
                            {
                                options.FlushInterval = flushInterval;

                                var elasticsearchOptions = options.Elasticsearch ?? new ElasticsearchOptions(url, "null");
                                reportConfiguration.Bind(elasticsearchOptions);
                                elasticsearchOptions.BaseUri = url;
                            });
                            break;

                        case "influxdb":
                            builder.Report.ToInfluxDb(options =>
                            {
                                options.FlushInterval = flushInterval;
                                var influxDbOptions = options.InfluxDb ?? new InfluxDbOptions();
                                reportConfiguration.Bind(influxDbOptions);
                                influxDbOptions.BaseUri = url;
                            });
                            break;

                        default:
                            throw new ArgumentException($"Reporter of type '{reportType}' is not supported");
                    }

                    services
                        .AddSingleton<IMetrics>(builder.Build())
                        .AddSingleton<IHostedService, ReportRunnerService>();
                });
        }
    }
}