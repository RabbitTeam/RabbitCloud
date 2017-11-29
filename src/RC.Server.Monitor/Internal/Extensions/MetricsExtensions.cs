using App.Metrics;
using App.Metrics.Gauge;
using App.Metrics.Timer;

namespace Rabbit.Cloud.Server.Monitor.Internal.Extensions
{
    internal static class MetricsExtensions
    {
        /// <summary>
        ///     Decrements the number of active web requests.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        public static void DecrementActiveRequests(this IMetrics metrics)
        {
            metrics.Measure.Counter.Decrement(MetricsRegistry.Counters.ActiveRequestCount);
        }

        /// <summary>
        ///     Increments the number of active active requests.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        public static void IncrementActiveRequests(this IMetrics metrics)
        {
            metrics.Measure.Counter.Increment(MetricsRegistry.Counters.ActiveRequestCount);
        }

        /// <summary>
        ///     Records the time taken to execute an API's endpoint in nanoseconds. Tags metrics by OAuth2 client id (if it exists)
        ///     and the endpoints route template.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="clientId">The client identifier, with track min/max durations by clientid.</param>
        /// <param name="serviceId">The serviceId of the endpoint.</param>
        /// <param name="elapsed">The time elapsed in executing the endpoints request.</param>
        public static void RecordEndpointsRequestTime(this IMetrics metrics, string clientId, string serviceId, long elapsed)
        {
            metrics.EndpointRequestTimer(serviceId).
                Record(
                    elapsed,
                    TimeUnit.Nanoseconds,
                    !string.IsNullOrWhiteSpace(clientId) ? clientId : null);
        }

        /// <summary>
        ///     Records metrics around unhanded exceptions, counts the total number of errors for each exception type.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="serviceId">The serviceId of the endpoint.</param>
        /// <param name="exception">The type of exception.</param>
        public static void RecordException(
            this IMetrics metrics,
            string serviceId,
            string exception)
        {
            var tags = new MetricTags(
                new[] { MiddlewareConstants.DefaultTagKeys.ServiceId, MiddlewareConstants.DefaultTagKeys.Exception },
                new[] { serviceId, exception });
            metrics.Measure.Counter.Increment(MetricsRegistry.Counters.UnhandledExceptionCount, tags);
        }

        /// <summary>
        ///     Records metrics about an request error, counts the total number of errors for each status code, measures the
        ///     rate and percentage of error requests tagging by client id (if it exists) the endpoints route template and
        ///     status code.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="serviceId">The serviceId of the endpoint.</param>
        /// <param name="statusCode">The status code.</param>
        public static void RecordRequestError(
            this IMetrics metrics,
            string serviceId,
            int statusCode)
        {
            CountOverallErrorRequestsByStatusCode(metrics, statusCode);

            metrics.Measure.Meter.Mark(MetricsRegistry.Meters.ErrorRequestRate);

            RecordEndpointsRequestErrors(metrics, serviceId, statusCode);
            RecordOverallPercentageOfErrorRequests(metrics);
            RecordEndpointsPercentageOfErrorRequests(metrics, serviceId);
        }

        private static void CountOverallErrorRequestsByStatusCode(IMetrics metrics, int statusCode)
        {
            var errorCounterTags = new MetricTags(MiddlewareConstants.DefaultTagKeys.StatusCode, statusCode.ToString());
            metrics.Measure.Counter.Increment(MetricsRegistry.Counters.TotalErrorRequestCount, errorCounterTags);
        }

        private static ITimer EndpointRequestTimer(this IMetrics metrics, string serviceId)
        {
            var tags = new MetricTags(MiddlewareConstants.DefaultTagKeys.ServiceId, serviceId);
            return metrics.Provider.Timer.Instance(MetricsRegistry.Timers.EndpointRequestTransactionDuration, tags);
        }

        private static void RecordEndpointsRequestErrors(IMetrics metrics, string serviceId, int statusCode)
        {
            var endpointErrorRequestTags = new MetricTags(MiddlewareConstants.DefaultTagKeys.ServiceId, serviceId);
            metrics.Measure.Meter.Mark(MetricsRegistry.Meters.EndpointErrorRequestRate, endpointErrorRequestTags);

            var endpointErrorRequestPerStatusCodeTags = new MetricTags(
                new[] { MiddlewareConstants.DefaultTagKeys.ServiceId, MiddlewareConstants.DefaultTagKeys.StatusCode },
                new[] { serviceId, statusCode.ToString() });

            metrics.Measure.Meter.Mark(
                MetricsRegistry.Meters.EndpointErrorRequestPerStatusCodeRate,
                endpointErrorRequestPerStatusCodeTags);
        }

        private static void RecordEndpointsPercentageOfErrorRequests(IMetrics metrics, string serviceId)
        {
            var tags = new MetricTags(MiddlewareConstants.DefaultTagKeys.ServiceId, serviceId);

            var endpointsErrorRate = metrics.Provider.Meter.Instance(MetricsRegistry.Meters.EndpointErrorRequestRate, tags);
            var endpointsRequestTransactionTime = metrics.EndpointRequestTimer(serviceId);

            metrics.Measure.Gauge.SetValue(
                MetricsRegistry.Gauges.EndpointOneMinuteErrorPercentageRate,
                tags,
                () => new HitPercentageGauge(endpointsErrorRate, endpointsRequestTransactionTime, m => m.OneMinuteRate));
        }

        private static void RecordOverallPercentageOfErrorRequests(IMetrics metrics)
        {
            var totalErrorRate = metrics.Provider.Meter.Instance(MetricsRegistry.Meters.ErrorRequestRate);
            var overallRequestTransactionTime = metrics.Provider.Timer.Instance(MetricsRegistry.Timers.RequestTransactionDuration);

            metrics.Measure.Gauge.SetValue(
                MetricsRegistry.Gauges.OneMinErrorPercentageRate,
                () => new HitPercentageGauge(totalErrorRate, overallRequestTransactionTime, m => m.OneMinuteRate));
        }
    }
}