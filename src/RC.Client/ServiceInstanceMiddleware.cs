using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;
using Rabbit.Cloud.Client.Internal;
using Rabbit.Cloud.Discovery.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client
{
    public class ServiceInstanceMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly RabbitClientOptions _options;
        private readonly IDiscoveryClient _discoveryClient;
        private readonly ILogger<ServiceInstanceMiddleware> _logger;

        public ServiceInstanceMiddleware(RabbitRequestDelegate next, IOptions<RabbitClientOptions> options, IDiscoveryClient discoveryClient, ILogger<ServiceInstanceMiddleware> logger)
        {
            _next = next;
            _options = options.Value;
            _discoveryClient = discoveryClient;
            _logger = logger;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var serviceRequestFeature = context.Features.Get<IServiceRequestFeature>();

            var requestOptions = serviceRequestFeature.RequestOptions;

            var instances = string.IsNullOrEmpty(serviceRequestFeature.ServiceName) ? null : _discoveryClient.GetInstances(serviceRequestFeature.ServiceName);

            if (instances == null || !instances.Any())
            {
                if (!string.IsNullOrEmpty(serviceRequestFeature.ServiceName))
                {
                    var exception = ExceptionUtilities.NotFindServiceInstance(serviceRequestFeature.ServiceName);
                    _logger.LogWarning(exception, exception.Message);
                }

                var request = context.Request;
                var serviceInstance = new ServiceInstance
                {
                    Host = request.Host,
                    Port = request.Port,
                    ServiceId = request.Path
                };
                serviceRequestFeature.GetServiceInstance = () => serviceInstance;
            }
            else
            {
                var chooser = _options.Choosers.Get(requestOptions.ServiceChooser) ?? _options.DefaultChooser;

                chooser = new FairServiceInstanceChooser(chooser);

                serviceRequestFeature.GetServiceInstance = () => chooser.Choose(instances);
            }

            await _next(context);
        }

        private class ServiceInstance : IServiceInstance
        {
            #region Implementation of IServiceInstance

            public string ServiceId { get; set; }
            public string Host { get; set; }
            public int Port { get; set; }
            public IDictionary<string, string> Metadata { get; set; }

            #endregion Implementation of IServiceInstance
        }
    }
}