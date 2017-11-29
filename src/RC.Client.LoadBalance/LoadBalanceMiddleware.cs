using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Abstractions;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Features;
using Rabbit.Cloud.Client.LoadBalance.Features;
using Rabbit.Cloud.Discovery.Abstractions;
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
        private readonly LoadBalanceOptions _loadBalanceOptions;

        public LoadBalanceMiddleware(RabbitRequestDelegate next, IDiscoveryClient discoveryClient, IOptions<LoadBalanceOptions> loadBalanceOptions, ILogger<LoadBalanceMiddleware> logger)
        {
            _next = next;
            _discoveryClient = discoveryClient;
            _logger = logger;
            _loadBalanceOptions = loadBalanceOptions.Value;
        }

        private IReadOnlyList<IServiceInstance> FindServiceInstances(ServiceUrl serviceUrl, ILoadBalanceFeature feature)
        {
            var serviceInstances = feature != null && feature.ServiceInstances.Any() ? feature.ServiceInstances.ToArray() : _discoveryClient.GetInstances(serviceUrl.Host)?.ToArray();

            if (serviceInstances == null || !serviceInstances.Any())
                throw new RabbitRpcException(RabbitRpcExceptionCode.Forbidden, $"according to url {serviceUrl}, can not find the service instance");

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                using (_logger.BeginScope("serviceInstances"))
                {
                    _logger.LogDebug("found service instances info:");
                    foreach (var instance in serviceInstances)
                    {
                        _logger.LogDebug(ServiceInstanceToString(instance));
                    }
                }
            }

            return serviceInstances;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var requestFeature = context.Features.Get<IRequestFeature>();
            var serviceUrl = requestFeature.ServiceUrl;

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"Prepare to parse the service instance for '{serviceUrl}'");

            var host = serviceUrl.Host;
            var port = serviceUrl.Port;

            try
            {
                var loadBalanceFeature = context.Features.Get<ILoadBalanceFeature>();
                if (loadBalanceFeature == null)
                {
                    loadBalanceFeature = new LoadBalanceFeature();
                    context.Features.Set(loadBalanceFeature);
                }

                var loadBalanceStrategy = GetLoadBalanceStrategy(loadBalanceFeature);

                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"use load balance strategy is {loadBalanceStrategy.GetType().Name}.");

                var serviceInstances = FindServiceInstances(serviceUrl, loadBalanceFeature);
                var serviceInstance = loadBalanceStrategy.Choose(host, serviceInstances);
                loadBalanceFeature.SelectedServiceInstance =
                    serviceInstance ??
                    throw new RabbitRpcException(RabbitRpcExceptionCode.Forbidden,
                        $"according load balance strategy:'{loadBalanceStrategy.GetType().Name}',unable to choose service instance.");

                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug(
                        $"selected service instance serviceId:{serviceInstance.ServiceId},host:{serviceInstance.Host},port:{serviceInstance.Port}");

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

        private static string ServiceInstanceToString(IServiceInstance serviceInstance)
        {
            return $"serviceId: {serviceInstance.ServiceId},host: {serviceInstance.Host},port: {serviceInstance.Port}";
        }

        private ILoadBalanceStrategy<string, IServiceInstance> GetLoadBalanceStrategy(ILoadBalanceFeature feature)
        {
            IEnumerable<ILoadBalanceStrategy<string, IServiceInstance>> GetStrategies()
            {
                if (feature?.LoadBalanceStrategy != null)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug("use feature LoadBalanceStrategy.");
                    yield return feature.LoadBalanceStrategy;
                }

                // by StrategyName
                {
                    var strategyName = feature?.Strategy;

                    if (!string.IsNullOrEmpty(strategyName))
                    {
                        var loadBalanceStrategies = _loadBalanceOptions.LoadBalanceStrategies;
                        if (loadBalanceStrategies.TryGetValue(strategyName, out var loadBalanceStrategy))
                        {
                            if (_logger.IsEnabled(LogLevel.Debug))
                                _logger.LogDebug($"use name is '{strategyName}' LoadBalanceStrategy.");
                            yield return loadBalanceStrategy;
                        }
                        else
                        {
                            _logger.LogWarning($"according name '{strategyName}' not unable to find LoadBalanceStrategy.");
                        }
                    }
                }

                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("use default LoadBalanceStrategy");
                yield return _loadBalanceOptions.DefaultLoadBalanceStrategy;
            }

            return GetStrategies().First(i => i != null);
        }

        #endregion Private Method
    }
}