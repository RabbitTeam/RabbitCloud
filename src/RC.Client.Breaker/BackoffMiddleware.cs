using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.LoadBalance.Features;
using Rabbit.Cloud.Discovery.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Breaker
{
    public class BackoffOptions
    {
        public Action<FailureContext> ErrorHandler { get; set; }
    }

    public class FailureContext
    {
        public FailureContext(FailureEntry entry, IRabbitContext rabbitContext, Exception exception)
        {
            Entry = entry;
            RabbitContext = rabbitContext;
            Exception = exception;
        }

        public IRabbitContext RabbitContext { get; }
        public Exception Exception { get; }
        public FailureEntry Entry { get; }
    }

    public class FailureEntry
    {
        private readonly ILogger _logger;

        public FailureEntry(IServiceInstance serviceInstance, ILogger logger)
        {
            _logger = logger;
            ServiceInstance = serviceInstance;
        }

        public IServiceInstance ServiceInstance { get; }
        public int Count { get; private set; }

        public TimeSpan TryInterval { get; private set; }
        public DateTime LastFailureTimeUtc { get; private set; }

        public void Mark(TimeSpan tryInterval)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation($"mark error,service instance:{FailureTable.ServiceInstanceToString(ServiceInstance)},tryInterval:{tryInterval.TotalSeconds}s.");

            Count = Count + 1;
            LastFailureTimeUtc = DateTime.UtcNow;
            TryInterval = tryInterval;
        }

        internal bool IsAvailable()
        {
            return LastFailureTimeUtc.Add(TryInterval) < DateTime.UtcNow;
        }
    }

    public class FailureTable : IDisposable
    {
        private readonly ILogger<FailureTable> _logger;
        private readonly ConcurrentDictionary<IServiceInstance, FailureEntry> _failures = new ConcurrentDictionary<IServiceInstance, FailureEntry>();
        private readonly Timer _timer;

        internal static string ServiceInstanceToString(IServiceInstance serviceInstance)
        {
            return $"serviceId:{serviceInstance.ServiceId},host:{serviceInstance.Host},port:{serviceInstance.Port}";
        }

        public FailureTable(ILogger<FailureTable> logger)
        {
            _logger = logger;
            _timer = new Timer(s =>
            {
                Compression();
            }, null, 1000 * 30, 1000 * 30);
        }

        private void Compression()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("begin compression...");

            // no data ignore
            if (!_failures.Any())
                return;

            // remove available service instance
            foreach (var serviceInstance in _failures.Where(i => i.Value.IsAvailable()).Select(i => i.Key).ToArray())
            {
                var result = _failures.TryRemove(serviceInstance, out var _);
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"remove service instance {ServiceInstanceToString(serviceInstance)}.success:{result}");
            }

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("end compression...");
        }

        public FailureEntry GetFailureEntry(IServiceInstance serviceInstance)
        {
            return _failures.GetOrAdd(serviceInstance, k => new FailureEntry(serviceInstance, _logger));
        }

        public void TryRemove(IServiceInstance serviceInstance)
        {
            if (!_failures.Any())
                return;

            if (!_failures.ContainsKey(serviceInstance))
                return;

            var success = _failures.TryRemove(serviceInstance, out var _);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation($"remove failure service instance:{ServiceInstanceToString(serviceInstance)}.success:{success}");
        }

        private IReadOnlyList<IServiceInstance> GetFailureServiceInstances()
        {
            return _failures.Where(i => !i.Value.IsAvailable()).Select(i => i.Key).ToArray();
        }

        public IReadOnlyCollection<IServiceInstance> GetHealthyServiceInstances(IReadOnlyCollection<IServiceInstance> serviceInstances)
        {
            if (!_failures.Any())
                return serviceInstances;

            var unavailables = GetFailureServiceInstances();

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                using (_logger.BeginScope("unavailable service instances:"))
                {
                    foreach (var serviceInstance in unavailables)
                    {
                        _logger.LogDebug(ServiceInstanceToString(serviceInstance));
                    }
                }
            }

            // ignore failure service instance
            serviceInstances = serviceInstances.Where(i => !unavailables.Contains(i)).ToArray();

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                using (_logger.BeginScope("available service instances:"))
                {
                    foreach (var serviceInstance in serviceInstances)
                    {
                        _logger.LogDebug(ServiceInstanceToString(serviceInstance));
                    }
                }
            }

            return serviceInstances;
        }

        #region IDisposable

        /// <inheritdoc />
        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _timer?.Dispose();
        }

        #endregion IDisposable
    }

    public class BackoffMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly FailureTable _failureTable;
        private readonly IDiscoveryClient _discoveryClient;
        private readonly BackoffOptions _options;

        public BackoffMiddleware(RabbitRequestDelegate next, IOptions<BackoffOptions> options, FailureTable failureTable, IDiscoveryClient discoveryClient)
        {
            _next = next;
            _failureTable = failureTable;
            _discoveryClient = discoveryClient;
            _options = options.Value;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var errorHandler = _options.ErrorHandler;

            if (errorHandler == null)
            {
                await _next(context);
                return;
            }

            var loadBalanceFeature = context.Features.Get<ILoadBalanceFeature>();
            if (loadBalanceFeature == null)
            {
                loadBalanceFeature = new LoadBalanceFeature();
                context.Features.Set(loadBalanceFeature);
            }

            try
            {
                var serviceName = context.Request.Url.Host;

                // get all service instances
                var serviceInstances = _discoveryClient.GetInstances(serviceName);

                loadBalanceFeature.ServiceInstances.Clear();

                // get available service instances
                foreach (var serviceInstance in _failureTable.GetHealthyServiceInstances(serviceInstances))
                    loadBalanceFeature.ServiceInstances.Add(serviceInstance);

                await _next(context);

                var currentServiceInstance = loadBalanceFeature.SelectedServiceInstance;
                if (currentServiceInstance == null)
                    return;

                _failureTable.TryRemove(currentServiceInstance);
            }
            catch (Exception e)
            {
                var currentServiceInstance = loadBalanceFeature.SelectedServiceInstance;
                if (currentServiceInstance == null)
                    throw;

                var failureEntry = _failureTable.GetFailureEntry(currentServiceInstance);
                var fastfailureContext = new FailureContext(failureEntry, context, e);

                errorHandler(fastfailureContext);
                throw;
            }
        }
    }
}