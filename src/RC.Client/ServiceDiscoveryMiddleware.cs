using Microsoft.Extensions.Logging;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;
using Rabbit.Cloud.Client.Features;
using Rabbit.Cloud.Discovery.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client
{
    public class ServiceDiscoveryMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly IDiscoveryClient _discoveryClient;
        private readonly ILogger<ServiceDiscoveryMiddleware> _logger;

        public ServiceDiscoveryMiddleware(RabbitRequestDelegate next, IDiscoveryClient discoveryClient, ILogger<ServiceDiscoveryMiddleware> logger)
        {
            _next = next;
            _discoveryClient = discoveryClient;
            _logger = logger;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var serviceDiscoveryFeature = context.Features.Get<IServiceDiscoveryFeature>();

            if (serviceDiscoveryFeature == null)
            {
                context.Features.Set(serviceDiscoveryFeature = new ServiceDiscoveryFeature
                {
                    ServiceId = context.Request.Host
                });
            }

            var serviceId = serviceDiscoveryFeature.ServiceId;

            var serviceInstances = _discoveryClient.GetInstances(serviceId);

            serviceDiscoveryFeature.ServiceInstances = serviceInstances;

            if (serviceInstances == null || !serviceInstances.Any())
            {
                var exception = ExceptionUtilities.NotFindServiceInstance(serviceId);
                _logger.LogWarning(exception, exception.Message);
            }

            await _next(context);
        }
    }
}