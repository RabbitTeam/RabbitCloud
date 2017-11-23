using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.ReservoirSampling.ExponentialDecay;
using App.Metrics.Timer;
using Rabbit.Cloud.Application.Abstractions;
using System.Collections.Generic;

namespace Rabbit.Cloud.Monitor
{
    public static class MetricsDefinition
    {
        private static readonly Dictionary<string, MetricValueOptionsBase> Options = new Dictionary<string, MetricValueOptionsBase>();

        public static CounterOptions TotalErrors => new CounterOptions
        {
            Name = "Errors"
        };

        public static CounterOptions TotalCount => new CounterOptions
        {
            Name = "TotalCount"
        };

        public static TimerOptions TotalRequestTimer => new TimerOptions
        {
            Name = "Request Time",
            MeasurementUnit = Unit.Requests,
            DurationUnit = TimeUnit.Milliseconds,
            RateUnit = TimeUnit.Milliseconds,
            Reservoir = () => new DefaultForwardDecayingReservoir(1028, 0.015)
        };

        public static TimerOptions GetServiceRequestTimer(this IRabbitContext rabbitContext)
        {
            var serviceName = $"{rabbitContext.Request.Url.Host}/{rabbitContext.Request.Url.Path}";
            var name = serviceName + " Request Time";

            var options = Get<TimerOptions>(name);
            if (options != null)
                return options;

            options = new TimerOptions
            {
                Name = name,
                MeasurementUnit = Unit.Requests,
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds,
                Reservoir = () => new DefaultForwardDecayingReservoir(1028, 0.015)
            };

            return Set(name, options);
        }

        public static CounterOptions GetServiceErrors(this IRabbitContext rabbitContext)
        {
            var serviceName = $"{rabbitContext.Request.Url.Host}/{rabbitContext.Request.Url.Path}";
            var name = serviceName + " Errors";

            var options = Get<CounterOptions>(name);
            if (options != null)
                return options;

            options = new CounterOptions
            {
                Name = name
            };

            return Set(name, options);
        }

        private static T Get<T>(string key) where T : MetricValueOptionsBase
        {
            return Options.TryGetValue(key, out var options) ? (T)options : default(T);
        }

        private static T Set<T>(string key, T options) where T : MetricValueOptionsBase
        {
            Options[key] = options;

            return options;
        }
    }
}