using App.Metrics;
using App.Metrics.Apdex;
using App.Metrics.Counter;
using App.Metrics.Gauge;
using App.Metrics.Histogram;
using App.Metrics.Meter;
using App.Metrics.Timer;
using System;

namespace Rabbit.Cloud.Server.Monitor.Internal
{
    internal static class MetricsRegistry
    {
        public static string ContextName = "Application.HttpRequests";

        public static class ApdexScores
        {
            public static readonly string ApdexMetricName = "Apdex";

            public static readonly Func<double, ApdexOptions> Apdex = apdexTSeconds => new ApdexOptions
            {
                Context = ContextName,
                Name = ApdexMetricName,
                ApdexTSeconds = apdexTSeconds
            };
        }

        public static class Counters
        {
            public static readonly CounterOptions ActiveRequestCount = new CounterOptions
            {
                Context = ContextName,
                Name = "Active",
                MeasurementUnit = Unit.Custom("Active Requests")
            };

            public static readonly CounterOptions TotalErrorRequestCount = new CounterOptions
            {
                Context = ContextName,
                Name = "Errors",
                ResetOnReporting = true,
                MeasurementUnit = Unit.Errors
            };

            public static readonly CounterOptions UnhandledExceptionCount = new CounterOptions
            {
                Context = ContextName,
                Name = "Exceptions",
                MeasurementUnit = Unit.Errors,
                ReportItemPercentages = false,
                ReportSetItems = false,
                ResetOnReporting = true
            };
        }

        public static class Gauges
        {
            public static readonly GaugeOptions EndpointOneMinuteErrorPercentageRate = new GaugeOptions
            {
                Context = ContextName,
                Name = "One Minute Error Percentage Rate Per Endpoint",
                MeasurementUnit = Unit.Requests
            };

            public static readonly GaugeOptions OneMinErrorPercentageRate = new GaugeOptions
            {
                Context = ContextName,
                Name = "One Minute Error Percentage Rate",
                MeasurementUnit = Unit.Requests
            };
        }

        public static class Histograms
        {
            public static readonly HistogramOptions PostRequestSizeHistogram = new HistogramOptions
            {
                Context = ContextName,
                Name = "POST Size",
                MeasurementUnit = Unit.Bytes
            };

            public static readonly HistogramOptions PutRequestSizeHistogram = new HistogramOptions
            {
                Context = ContextName,
                Name = "PUT Size",
                MeasurementUnit = Unit.Bytes
            };
        }

        public static class Meters
        {
            public static readonly MeterOptions EndpointErrorRequestPerStatusCodeRate = new MeterOptions
            {
                Context = ContextName,
                Name = "Error Rate Per Endpoint And Status Code",
                MeasurementUnit = Unit.Requests
            };

            public static readonly MeterOptions EndpointErrorRequestRate = new MeterOptions
            {
                Context = ContextName,
                Name = "Error Rate Per Endpoint",
                MeasurementUnit = Unit.Requests
            };

            public static readonly MeterOptions ErrorRequestRate = new MeterOptions
            {
                Context = ContextName,
                Name = "Error Rate",
                MeasurementUnit = Unit.Requests
            };
        }

        public static class Timers
        {
            public static readonly TimerOptions EndpointRequestTransactionDuration = new TimerOptions
            {
                Context = ContextName,
                Name = "Transactions Per Endpoint",
                MeasurementUnit = Unit.Requests
            };

            public static readonly TimerOptions RequestTransactionDuration = new TimerOptions
            {
                Context = ContextName,
                Name = "Transactions",
                MeasurementUnit = Unit.Requests
            };
        }

        /*        private static readonly Dictionary<string, MetricValueOptionsBase> Options = new Dictionary<string, MetricValueOptionsBase>();

                private static T Get<T>(string key) where T : MetricValueOptionsBase
                {
                    return Options.TryGetValue(key, out var options) ? (T)options : default(T);
                }

                private static T Set<T>(string key, T options) where T : MetricValueOptionsBase
                {
                    Options[key] = options;

                    return options;
                }*/
    }
}