using Consul;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Discovery.Abstractions;
using Rabbit.Cloud.Discovery.Consul.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Rabbit.Cloud.Discovery.Consul.Discovery
{
    public class ConsulDiscoveryClient : ConsulService, IDiscoveryClient
    {
        public class InstanceEntry
        {
            public InstanceEntry()
            {
                Instances = new List<IServiceInstance>();
            }

            public ICollection<IServiceInstance> Instances { get; }
            public ulong Index { get; set; }
        }

        private readonly ConcurrentDictionary<string, ICollection<IServiceInstance>> _instances = new ConcurrentDictionary<string, ICollection<IServiceInstance>>(StringComparer.OrdinalIgnoreCase);

        #region Constructor

        public ConsulDiscoveryClient(IOptionsMonitor<ConsulOptions> consulOptionsMonitor, ILogger<ConsulDiscoveryClient> logger)
            : base(consulOptionsMonitor)
        {
            var consulClient = ConsulClient;
            Task.Factory.StartNew(async () =>
            {
                var healthEndpoint = consulClient.Health;

                ulong index = 0;
                //watcher
                while (!Disposed)
                {
                    var result = await healthEndpoint.State(HealthStatus.Critical, new QueryOptions { WaitIndex = index });

                    //timeout ignore
                    if (index == result.LastIndex)
                        continue;
                    index = result.LastIndex;

                    var response = result.Response;
                    //_instances no data ignore
                    if (!_instances.Any())
                        continue;

                    // critical to delete
                    var criticalServices = response.GroupBy(i => i.ServiceName).Select(i => i.Key).ToArray();
                    if (logger.IsEnabled(LogLevel.Debug))
                        logger.LogDebug($"ready delete critical service info ,{string.Join(",", criticalServices)}");
                    foreach (var serviceName in criticalServices)
                        _instances.TryRemove(serviceName, out var _);
                }
            });
            Task.Factory.StartNew(async () =>
            {
                var catalogEndpoint = consulClient.Catalog;

                ulong index = 0;
                //watcher
                while (!Disposed)
                {
                    var result = await catalogEndpoint.Services(new QueryOptions { WaitIndex = index });

                    //timeout ignore
                    if (index == result.LastIndex)
                        continue;

                    index = result.LastIndex;

                    var response = result.Response;
                    Services = response.Where(i => i.Value.Contains(ConsulUtil.ServicePrefix)).Select(i => i.Key).ToArray();
                }
            });
        }

        #endregion Constructor

        #region Implementation of IDiscoveryClient

        public string Description => "Rabbit Cloud Consul Client";
        public IReadOnlyCollection<string> Services { get; private set; }

        public IReadOnlyCollection<IServiceInstance> GetInstances(string serviceId)
        {
            if (_instances.TryGetValue(serviceId, out var instances))
                return instances.ToArray();

            Task.Run(async () =>
            {
                instances = new List<IServiceInstance>();

                var result = await ConsulClient.Health.Service(serviceId, null, true);
                foreach (var instance in result.Response.Where(i => i.Checks.All(c => c.Status.Status == HealthStatus.Passing.Status)).Select(i => ConsulUtil.Create(i.Service)).Where(i => i != null))
                {
                    instances.Add(instance);
                }
                _instances.TryAdd(serviceId, instances);
            }).Wait();

            return instances.ToArray();
        }

        #endregion Implementation of IDiscoveryClient
    }
}