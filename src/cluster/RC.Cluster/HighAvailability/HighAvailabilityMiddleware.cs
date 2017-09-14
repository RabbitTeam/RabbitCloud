using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RC.Discovery.Client.Abstractions;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RC.Cluster.HighAvailability
{
    public class HighAvailabilityMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly HighAvailabilityOptions _highAvailabilityOptions;
        private readonly ILogger<HighAvailabilityMiddleware> _logger;

        public HighAvailabilityMiddleware(RabbitRequestDelegate next, IOptions<HighAvailabilityOptions> highAvailabilityOptions, ILogger<HighAvailabilityMiddleware> logger)
        {
            _next = next;
            _highAvailabilityOptions = highAvailabilityOptions.Value;
            _logger = logger;
        }

        public async Task Invoke(RabbitContext context)
        {
            switch (_highAvailabilityOptions.Strategy)
            {
                case HighAvailabilityStrategy.Failover:
                    {
                        var tryCount = _highAvailabilityOptions.RetryCount;
                        for (var i = 0; i < tryCount; i++)
                        {
                            var isLast = i == tryCount - 1;
                            try
                            {
                                await _next(context);
                                break;
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, $"HighAvailability try count {i + 1}.");
                                if (!isLast)
                                {
                                    context.Request.RequestMessage = CreateHttpRequestMessage(context.Request.RequestMessage);

                                    continue;
                                }
                                _logger.LogError(e, "service invoke failure.");
                                throw;
                            }
                        }
                        break;
                    }

                default:
                    await _next(context);
                    break;
            }
        }

        private static HttpRequestMessage CreateHttpRequestMessage(HttpRequestMessage requestMessage)
        {
            var newRequestMessage =
                new HttpRequestMessage(requestMessage.Method, requestMessage.RequestUri)
                {
                    Content = requestMessage.Content
                };

            foreach (var header in requestMessage.Headers)
            {
                newRequestMessage.Headers.Add(header.Key, header.Value);
            }
            foreach (var property in requestMessage.Properties)
            {
                newRequestMessage.Properties.Add(property.Key, property.Value);
            }
            newRequestMessage.Version = requestMessage.Version;
            return newRequestMessage;
        }
    }
}