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
        public Func<FailureContext, bool> IsFailure { get; set; }
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
        public FailureEntry(IServiceInstance serviceInstance, TimeSpan tryInterval)
        {
            ServiceInstance = serviceInstance;
            TryInterval = tryInterval;
            LastFailureTimeUtc = DateTime.UtcNow;
            Count = 1;
        }

        public IServiceInstance ServiceInstance { get; }
        public int Count { get; private set; }

        public TimeSpan TryInterval { get; }
        public DateTime LastFailureTimeUtc { get; private set; }

        internal void Mark()
        {
            Count = Count + 1;
            LastFailureTimeUtc = DateTime.UtcNow;
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

        public FailureTable(ILogger<FailureTable> logger)
        {
            _logger = logger;
            _timer = new Timer(s =>
              {
                  // no data ignore
                  if (!_failures.Any())
                      return;

                  // remove available service instance
                  foreach (var serviceInstance in _failures.Where(i => i.Value.IsAvailable()).Select(i => i.Key).ToArray())
                      _failures.TryRemove(serviceInstance, out var _);
              }, null, 1000 * 10, 1000 * 10);
        }

        public FailureEntry GetFailureEntry(IServiceInstance serviceInstance)
        {
            return _failures.TryGetValue(serviceInstance, out var value) ? value : null;
        }

        public void Mark(FailureEntry entry)
        {
            var isFirst = false;
            var value = _failures.GetOrAdd(entry.ServiceInstance, k =>
            {
                isFirst = true;
                return entry;
            });
            if (!isFirst)
                value.Mark();
        }

        public void Remove(FailureEntry entry)
        {
            _failures.TryRemove(entry.ServiceInstance, out var _);
        }

        private IReadOnlyList<IServiceInstance> GetFailureServiceInstances()
        {
            return _failures.Where(i => !i.Value.IsAvailable()).Select(i => i.Key).ToArray();
        }

        public IReadOnlyList<IServiceInstance> GetHealthyServiceInstances(IReadOnlyList<IServiceInstance> serviceInstances)
        {
            if (!_failures.Any())
                return serviceInstances;

            var unavailables = GetFailureServiceInstances();
            return serviceInstances.Where(i => !unavailables.Contains(i)).ToArray();
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
            var isFailure = _options.IsFailure;
            var loadBalanceFeature = context.Features.Get<ILoadBalanceFeature>();
            if (loadBalanceFeature == null)
            {
                loadBalanceFeature = new LoadBalanceFeature();
                context.Features.Set(loadBalanceFeature);
            }

            if (isFailure == null)
            {
                await _next(context);
                return;
            }

            try
            {
                var serviceName = context.Request.Url.Host;
                var serviceInstances = _discoveryClient.GetInstances(serviceName);

                loadBalanceFeature.ServiceInstances.Clear();
                foreach (var serviceInstance in _failureTable.GetHealthyServiceInstances(serviceInstances.ToArray()))
                    loadBalanceFeature.ServiceInstances.Add(serviceInstance);

                await _next(context);
            }
            catch (Exception e)
            {
                var currentServiceInstance = loadBalanceFeature.SelectedServiceInstance;
                if (currentServiceInstance == null)
                    return;

                var failureEntry = _failureTable.GetFailureEntry(currentServiceInstance) ?? new FailureEntry(currentServiceInstance, TimeSpan.FromSeconds(30));

                var fastfailureContext = new FailureContext(failureEntry, context, e);

                // ignore
                if (!isFailure(fastfailureContext))
                    return;

                _failureTable.Mark(fastfailureContext.Entry);
            }
        }
    }
}