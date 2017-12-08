using Microsoft.Extensions.Logging;
using Rabbit.Cloud.Abstractions;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Features;
using Rabbit.Cloud.Client.LoadBalance.Features;
using Rabbit.Cloud.Discovery.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.LoadBalance
{
    public class LoadBalanceMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly IDiscoveryClient _discoveryClient;
        private readonly ILogger<LoadBalanceMiddleware> _logger;

        public LoadBalanceMiddleware(RabbitRequestDelegate next, IDiscoveryClient discoveryClient, ILogger<LoadBalanceMiddleware> logger)
        {
            _next = next;
            _discoveryClient = discoveryClient;
            _logger = logger;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var requestFeature = context.Features.Get<IRequestFeature>();
            var serviceUrl = requestFeature.ServiceUrl;

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"Prepare to parse the service instance for '{serviceUrl}'");

            var loadBalanceFeature = context.Features.Get<ILoadBalanceFeature>();

            var serviceInstanceChooser = loadBalanceFeature.ServiceInstanceChooser;
            var strategy = loadBalanceFeature.Strategy;

            var host = serviceUrl.Host;
            var port = serviceUrl.Port;

            var serviceInstances = _discoveryClient.GetInstances(host).ToArray();

            IReadOnlyList<IServiceInstance> GetAvailableServiceInstances(IServiceInstance ignoreServiceInstance)
            {
                return serviceInstances.Where(i => i != ignoreServiceInstance).ToArray();
            }

            try
            {
                var maxAutoRetriesNextServer = strategy.MaxAutoRetriesNextServer;
                if (serviceInstances.Length < 2)
                    maxAutoRetriesNextServer = 0;

                IServiceInstance serviceInstance = null;
                for (var i = 0; i <= maxAutoRetriesNextServer; i++)
                {
                    var currentServiceInstances = GetAvailableServiceInstances(serviceInstance);

                    if (currentServiceInstances == null || !currentServiceInstances.Any())
                        throw new RabbitRpcException(RabbitRpcExceptionCode.Forbidden, $"Can not find service '{host}' instances.");

                    serviceInstance = serviceInstanceChooser.Choose(host, currentServiceInstances);

                    serviceUrl.Host = serviceInstance.Host;
                    serviceUrl.Port = serviceInstance.Port;

                    for (var z = 0; z < strategy.MaxAutoRetries; z++)
                    {
                        try
                        {
                            await _next(context);
                            return;
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Execution failed.");
                        }
                    }
                }
            }
            finally
            {
                serviceUrl.Host = host;
                serviceUrl.Port = port;
            }
        }
    }
}