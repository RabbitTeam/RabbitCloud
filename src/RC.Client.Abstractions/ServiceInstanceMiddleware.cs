using Microsoft.Extensions.Options;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;
using Rabbit.Cloud.Discovery.Abstractions;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Abstractions
{
    public class ServiceInstanceMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly RabbitClientOptions _options;
        private readonly IDiscoveryClient _discoveryClient;

        public ServiceInstanceMiddleware(RabbitRequestDelegate next, IOptions<RabbitClientOptions> options, IDiscoveryClient discoveryClient)
        {
            _next = next;
            _options = options.Value;
            _discoveryClient = discoveryClient;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var serviceRequestFeature = context.Features.Get<IServiceRequestFeature>();

            var requestOptions = serviceRequestFeature.RequestOptions;

            var instances = _discoveryClient.GetInstances(serviceRequestFeature.ServiceName);

            var chooser = _options.Choosers.Get(requestOptions.ServiceChooser) ?? _options.DefaultChooser;

            serviceRequestFeature.Chooser = chooser;
            serviceRequestFeature.ServiceInstances = instances;

            await _next(context);
        }
    }
}