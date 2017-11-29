using App.Metrics;
using App.Metrics.Apdex;
using App.Metrics.Counter;
using App.Metrics.Gauge;
using App.Metrics.Meter;
using App.Metrics.Timer;
using System;

namespace Rabbit.Cloud.Server.Monitor.Internal
{
    internal static class MetricsRegistry
    {
        public static string ContextName = "RabbitApplication.ServiceRequests";

        public static class ApdexScores
        {
            public static readonly string ApdexMetricName = "Apdex";

            public static Func<double, ApdexOptions> Apdex { get; } = apdexTSeconds => new ApdexOptions
            {
                Context = ContextName,
                Name = ApdexMetricName,
                ApdexTSeconds = apdexTSeconds
            };
        }

        public static class Counters
        {
            public static CounterOptions ActiveRequestCount { get; } = new CounterOptions
            {
                Context = ContextName,
                Name = "Active",
                MeasurementUnit = Unit.Custom("Active Requests")
            };

            public static CounterOptions TotalErrorRequestCount { get; } = new CounterOptions
            {
                Context = ContextName,
                Name = "Errors",
                ResetOnReporting = true,
                MeasurementUnit = Unit.Errors
            };

            public static CounterOptions UnhandledExceptionCount { get; } = new CounterOptions
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
            public static GaugeOptions EndpointOneMinuteErrorPercentageRate { get; } = new GaugeOptions
            {
                Context = ContextName,
                Name = "One Minute Error Percentage Rate Per Endpoint",
                MeasurementUnit = Unit.Requests
            };

            public static GaugeOptions OneMinErrorPercentageRate { get; } = new GaugeOptions
            {
                Context = ContextName,
                Name = "One Minute Error Percentage Rate",
                MeasurementUnit = Unit.Requests
            };
        }

        public static class Meters
        {
            public static MeterOptions EndpointErrorRequestPerStatusCodeRate { get; } = new MeterOptions
            {
                Context = ContextName,
                Name = "Error Rate Per Endpoint And Status Code",
                MeasurementUnit = Unit.Requests
            };

            public static MeterOptions EndpointErrorRequestRate { get; } = new MeterOptions
            {
                Context = ContextName,
                Name = "Error Rate Per Endpoint",
                MeasurementUnit = Unit.Requests
            };

            public static MeterOptions ErrorRequestRate { get; } = new MeterOptions
            {
                Context = ContextName,
                Name = "Error Rate",
                MeasurementUnit = Unit.Requests
            };
        }

        public static class Timers
        {
            public static TimerOptions EndpointRequestTransactionDuration { get; } = new TimerOptions
            {
                Context = ContextName,
                Name = "Transactions Per Endpoint",
                MeasurementUnit = Unit.Requests
            };

            public static TimerOptions RequestTransactionDuration { get; } = new TimerOptions
            {
                Context = ContextName,
                Name = "Transactions",
                MeasurementUnit = Unit.Requests
            };
        }
    }
}