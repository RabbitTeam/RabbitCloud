using Microsoft.Extensions.Logging;
using Rabbit.Cloud.Discovery.Abstractions;
using RC.Discovery.Client.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Discovery.Client.Middlewares
{
    public class ServiceAddressResolveMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly ILogger<ServiceAddressResolveMiddleware> _logger;
        private readonly IDiscoveryClient _discoveryClient;

        private static readonly Random Random = new Random();

        public ServiceAddressResolveMiddleware(RabbitRequestDelegate next, ILogger<ServiceAddressResolveMiddleware> logger, IDiscoveryClient discoveryClient)
        {
            _next = next;
            _logger = logger;
            _discoveryClient = discoveryClient;
        }

        public async Task Invoke(RabbitContext context)
        {
            var request = context.Request;
            var current = request.RequestUri;
            if (!current.IsDefaultPort)
            {
                await _next(context);
                return;
            }

            try
            {
                request.RequestUri = LookupService(current);
                await _next(context);
            }
            finally
            {
                request.RequestUri = current;
            }
        }

        private Uri LookupService(Uri current)
        {
            _logger?.LogDebug("LookupService({0})", current.ToString());

            var instances = _discoveryClient.GetInstances(current.Host);

            if (instances != null && instances.Any())
            {
                var index = Random.Next(instances.Count);
                current = new Uri(instances.ElementAt(index).Uri, current.PathAndQuery);
            }
            _logger?.LogDebug("LookupService() returning {0} ", current.ToString());
            return current;
        }
    }
}