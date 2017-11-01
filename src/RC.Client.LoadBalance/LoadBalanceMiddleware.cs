using Microsoft.Extensions.Options;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Features;
using Rabbit.Cloud.Client.LoadBalance.Features;
using Rabbit.Cloud.Discovery.Abstractions;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.LoadBalance
{
    public class LoadBalanceMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly IDiscoveryClient _discoveryClient;
        private readonly LoadBalanceOptions _loadBalanceOptions;

        public LoadBalanceMiddleware(RabbitRequestDelegate next, IDiscoveryClient discoveryClient, IOptions<LoadBalanceOptions> loadBalanceOptions)
        {
            _next = next;
            _discoveryClient = discoveryClient;
            _loadBalanceOptions = loadBalanceOptions.Value;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var requestFeature = context.Features.Get<IRequestFeature>();
            var serviceUrl = requestFeature.ServiceUrl;

            var host = serviceUrl.Host;
            var port = serviceUrl.Port;

            try
            {
                var serviceInstances = _discoveryClient.GetInstances(host);

                var loadBalanceFeature = context.Features.Get<ILoadBalanceFeature>();
                var loadBalanceStrategy = GetLoadBalanceStrategy(loadBalanceFeature?.Strategy);
                var serviceInstance = loadBalanceStrategy.Choose(host, serviceInstances);
                requestFeature.ServiceUrl.Host = serviceInstance.Host;
                requestFeature.ServiceUrl.Port = serviceInstance.Port;

                await _next(context);
            }
            finally
            {
                serviceUrl.Host = host;
                serviceUrl.Port = port;
            }
        }

        #region Private Method

        private ILoadBalanceStrategy<string, IServiceInstance> GetLoadBalanceStrategy(string strategyName)
        {
            var loadBalanceStrategies = _loadBalanceOptions.LoadBalanceStrategies;
            if (!string.IsNullOrEmpty(strategyName) && loadBalanceStrategies.TryGetValue(strategyName, out var loadBalanceStrategy))
            {
                return loadBalanceStrategy;
            }
            return _loadBalanceOptions.DefaultLoadBalanceStrategy;
        }

        #endregion Private Method
    }
}