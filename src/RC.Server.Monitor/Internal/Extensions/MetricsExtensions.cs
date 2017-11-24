using App.Metrics;
using App.Metrics.Gauge;
using App.Metrics.Timer;

namespace Rabbit.Cloud.Server.Monitor.Internal.Extensions
{
    internal static class MetricsExtensions
    { /// <summary>
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

        /*/// <summary>
        ///     Records metrics about a Clients HTTP request error, counts the total number of errors for each status code,
        ///     measures the
        ///     rate and percentage of HTTP error requests tagging by client id (if it exists) the endpoints route template and
        ///     HTTP status code.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="routeTemplate">The route template of the endpoint.</param>
        /// <param name="httpStatusCode">The HTTP status code.</param>
        /// <param name="clientId">The OAuth2 client identifier.</param>
        public static void RecordClientHttpRequestError(
            this IMetrics metrics,
            string routeTemplate,
            int httpStatusCode,
            string clientId)
        {
            var tags = new MetricTags(
                new[] { MiddlewareConstants.DefaultTagKeys.ClientId, MiddlewareConstants.DefaultTagKeys.Route, MiddlewareConstants.DefaultTagKeys.HttpStatusCode },
                new[] { clientId, routeTemplate, httpStatusCode.ToString() });

            metrics.Measure.Meter.Mark(OAuthRequestMetricsRegistry.Meters.ErrorRate, tags);
        }

        public static void RecordClientRequestRate(this IMetrics metrics, string routeTemplate, string clientId)
        {
            var tags = new MetricTags(new[] { MiddlewareConstants.DefaultTagKeys.ClientId, MiddlewareConstants.DefaultTagKeys.Route }, new[] { clientId, routeTemplate });

            metrics.Measure.Meter.Mark(OAuthRequestMetricsRegistry.Meters.RequestRate, tags);
        }*/

        /// <summary>
        ///     Records the time taken to execute an API's endpoint in nanoseconds. Tags metrics by OAuth2 client id (if it exists)
        ///     and the endpoints route template.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="clientId">The OAuth2 client identifier, with track min/max durations by clientid.</param>
        /// <param name="routeTemplate">The route template of the endpoint.</param>
        /// <param name="elapsed">The time elapsed in executing the endpoints request.</param>
        public static void RecordEndpointsRequestTime(this IMetrics metrics, string clientId, string routeTemplate, long elapsed)
        {
            metrics.EndpointRequestTimer(routeTemplate).
                    Record(
                        elapsed,
                        TimeUnit.Nanoseconds,
                        clientId.IsPresent() ? clientId : null);
        }

        /// <summary>
        ///     Records metrics around unhanded exceptions, counts the total number of errors for each exception type.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="routeTemplate">The route template of the endpoint.</param>
        /// <param name="exception">The type of exception.</param>
        public static void RecordException(
            this IMetrics metrics,
            string routeTemplate,
            string exception)
        {
            var tags = new MetricTags(
                new[] { MiddlewareConstants.DefaultTagKeys.Route, MiddlewareConstants.DefaultTagKeys.Exception },
                new[] { routeTemplate, exception });
            metrics.Measure.Counter.Increment(MetricsRegistry.Counters.UnhandledExceptionCount, tags);
        }

        /// <summary>
        ///     Records metrics about an HTTP request error, counts the total number of errors for each status code, measures the
        ///     rate and percentage of HTTP error requests tagging by client id (if it exists) the endpoints route template and
        ///     HTTP status code.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="routeTemplate">The route template of the endpoint.</param>
        /// <param name="httpStatusCode">The HTTP status code.</param>
        public static void RecordHttpRequestError(
            this IMetrics metrics,
            string routeTemplate,
            int httpStatusCode)
        {
            CountOverallErrorRequestsByHttpStatusCode(metrics, httpStatusCode);

            metrics.Measure.Meter.Mark(MetricsRegistry.Meters.ErrorRequestRate);

            RecordEndpointsHttpRequestErrors(metrics, routeTemplate, httpStatusCode);
            RecordOverallPercentageOfErrorRequests(metrics);
            RecordEndpointsPercentageOfErrorRequests(metrics, routeTemplate);
        }

        /*/// <summary>
        ///     Records a metric for the size of a HTTP POST requests.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="value">The value.</param>
        /// <param name="clientId">The OAuth2 client identifier.</param>
        /// <param name="routeTemplate">The route template of the endpoint.</param>
        public static void UpdateClientPostRequestSize(this IMetrics metrics, long value, string clientId, string routeTemplate)
        {
            var tags = new MetricTags(new[] { MiddlewareConstants.DefaultTagKeys.ClientId, MiddlewareConstants.DefaultTagKeys.Route }, new[] { clientId, routeTemplate });
            metrics.Measure.Histogram.Update(OAuthRequestMetricsRegistry.Histograms.PostRequestSizeHistogram, tags, value);
        }

        /// <summary>
        ///     Records a metric for the size of a HTTP PUT requests.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="value">The value.</param>
        /// <param name="clientId">The OAuth2 client identifier to tag the histogram values.</param>
        /// <param name="routeTemplate">The route template of the endpoint.</param>
        public static void UpdateClientPutRequestSize(this IMetrics metrics, long value, string clientId, string routeTemplate)
        {
            var tags = new MetricTags(new[] { MiddlewareConstants.DefaultTagKeys.ClientId, MiddlewareConstants.DefaultTagKeys.Route }, new[] { clientId, routeTemplate });
            metrics.Measure.Histogram.Update(OAuthRequestMetricsRegistry.Histograms.PutRequestSizeHistogram, tags, value);
        }*/

        /// <summary>
        ///     Records a metric for the size of a HTTP POST requests.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="value">The value.</param>
        public static void UpdatePostRequestSize(this IMetrics metrics, long value)
        {
            metrics.Measure.Histogram.Update(MetricsRegistry.Histograms.PostRequestSizeHistogram, value);
        }

        /// <summary>
        ///     Records a metric for the size of a HTTP PUT requests.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="value">The value.</param>
        public static void UpdatePutRequestSize(this IMetrics metrics, long value)
        {
            metrics.Measure.Histogram.Update(MetricsRegistry.Histograms.PutRequestSizeHistogram, value);
        }

        private static void CountOverallErrorRequestsByHttpStatusCode(IMetrics metrics, int httpStatusCode)
        {
            var errorCounterTags = new MetricTags(MiddlewareConstants.DefaultTagKeys.HttpStatusCode, httpStatusCode.ToString());
            metrics.Measure.Counter.Increment(MetricsRegistry.Counters.TotalErrorRequestCount, errorCounterTags);
        }

        private static ITimer EndpointRequestTimer(this IMetrics metrics, string routeTemplate)
        {
            var tags = new MetricTags(MiddlewareConstants.DefaultTagKeys.Route, routeTemplate);
            return metrics.Provider.Timer.Instance(MetricsRegistry.Timers.EndpointRequestTransactionDuration, tags);
        }

        private static void RecordEndpointsHttpRequestErrors(IMetrics metrics, string routeTemplate, int httpStatusCode)
        {
            var endpointErrorRequestTags = new MetricTags(MiddlewareConstants.DefaultTagKeys.Route, routeTemplate);
            metrics.Measure.Meter.Mark(MetricsRegistry.Meters.EndpointErrorRequestRate, endpointErrorRequestTags);

            var endpointErrorRequestPerStatusCodeTags = new MetricTags(
                new[] { MiddlewareConstants.DefaultTagKeys.Route, MiddlewareConstants.DefaultTagKeys.HttpStatusCode },
                new[] { routeTemplate, httpStatusCode.ToString() });

            metrics.Measure.Meter.Mark(
                MetricsRegistry.Meters.EndpointErrorRequestPerStatusCodeRate,
                endpointErrorRequestPerStatusCodeTags);
        }

        private static void RecordEndpointsPercentageOfErrorRequests(IMetrics metrics, string routeTemplate)
        {
            var tags = new MetricTags(MiddlewareConstants.DefaultTagKeys.Route, routeTemplate);

            var endpointsErrorRate = metrics.Provider.Meter.Instance(MetricsRegistry.Meters.EndpointErrorRequestRate, tags);
            var endpointsRequestTransactionTime = metrics.EndpointRequestTimer(routeTemplate);

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