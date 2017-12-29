using System;

namespace Rabbit.Cloud.Client.Abstractions
{
    public static class ExceptionUtilities
    {
        public static RabbitClientException NotFindServiceInstance(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName))
                throw new ArgumentNullException(nameof(serviceName));
            throw new RabbitClientException($"According to the service name '{serviceName}', can not find the service instance.", 400);
        }

        public static RabbitClientException ServiceRequestTimeout(string url)
        {
            throw new RabbitClientException($"Request '{url}' timed out", 503);
        }
    }
}