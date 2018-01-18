using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;
using Rabbit.Cloud.Client.Features;
using Rabbit.Cloud.Client.ServiceInstanceChooser;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client
{
    public class LoadBalanceMiddleware
    {
        private readonly RabbitRequestDelegate _next;

        public LoadBalanceMiddleware(RabbitRequestDelegate next)
        {
            _next = next;
        }

        private static readonly IServiceInstanceChooser DefaultServiceInstanceChooser = new RandomServiceInstanceChooser();

        public async Task Invoke(IRabbitContext context)
        {
            var serviceDiscoveryFeature = context.Features.Get<IServiceDiscoveryFeature>();
            var serviceInstances = serviceDiscoveryFeature?.ServiceInstances;

            var request = context.Request;

            if (serviceInstances == null || !serviceDiscoveryFeature.ServiceInstances.Any())
            {
                await _next(context);
            }
            else
            {
                var loadBalanceFeature = context.Features.Get<ILoadBalanceFeature>();
                if (loadBalanceFeature == null)
                    context.Features.Set(loadBalanceFeature = new LoadBalanceFeature());

                var serviceInstanceChooser = loadBalanceFeature.ServiceInstanceChooser ?? DefaultServiceInstanceChooser;

                var requestInstance = serviceInstanceChooser.Choose(serviceInstances);

                loadBalanceFeature.RequestInstance = requestInstance;

                var host = request.Host;
                var port = request.Port;

                request.Host = requestInstance.Host;
                request.Port = requestInstance.Port;

                try
                {
                    await _next(context);
                }
                finally
                {
                    request.Host = host;
                    request.Port = port;
                }
            }
        }
    }
}