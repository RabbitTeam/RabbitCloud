using Consul;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Discovery.Abstractions;
using Rabbit.Cloud.Discovery.Consul.Internal;
using Rabbit.Cloud.Discovery.Consul.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Rabbit.Cloud.Discovery.Consul.Discovery
{
    public class ConsulDiscoveryClient : ConsulService, IDiscoveryClient
    {
        private readonly ServiceNameResolver _serviceNameResolver;

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

        private ManualResetEventSlim _manualResetEventSlim = new ManualResetEventSlim();

        public ConsulDiscoveryClient(IOptionsMonitor<ConsulOptions> consulOptionsMonitor, ILogger<ConsulDiscoveryClient> logger, ServiceNameResolver serviceNameResolver)
            : base(consulOptionsMonitor)
        {
            _serviceNameResolver = serviceNameResolver;
            var consulClient = ConsulClient;
            Task.Factory.StartNew(async () =>
            {
                var healthEndpoint = consulClient.Health;

                ulong index = 0;
                //watcher
                while (!Disposed)
                {
                    try
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
                    catch (Exception e)
                    {
                        logger.LogError(e, "监听服务健康度时发生了错误。");
                    }
                }
            });
            Task.Factory.StartNew(async () =>
            {
                var catalogEndpoint = consulClient.Catalog;

                ulong index = 0;
                //watcher
                while (!Disposed)
                {
                    try
                    {
                        var result = await catalogEndpoint.Services(new QueryOptions { WaitIndex = index });

                        //timeout ignore
                        if (index == result.LastIndex)
                            continue;

                        index = result.LastIndex;

                        var response = result.Response;
                        Services = response
                            .Where(i => i.Value.Contains(ConsulUtil.ServicePrefix))
                            .Select(i => i.Key)
                            .Distinct(StringComparer.OrdinalIgnoreCase) // consul not case sensitive
                            .ToArray();
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "监听服务时发生了错误。");
                    }
                }
            });
        }

        #endregion Constructor

        #region Implementation of IDiscoveryClient

        public string Description => "Rabbit Cloud Consul Client";

        private IReadOnlyList<string> _services;

        public IReadOnlyList<string> Services
        {
            get
            {
                _manualResetEventSlim?.Wait();
                _manualResetEventSlim = null;
                return _services;
            }
            private set
            {
                _services = value;
                _manualResetEventSlim?.Set();
            }
        }

        public IReadOnlyList<IServiceInstance> GetInstances(string serviceId)
        {
            var consulServiceName = _serviceNameResolver.GetConsulNameByLocalName(serviceId);
            if (_instances.TryGetValue(consulServiceName, out var instances))
                return instances.ToArray();

            Task.Run(async () =>
            {
                instances = new List<IServiceInstance>();

                var result = await ConsulClient.Health.Service(consulServiceName, null, true);
                foreach (var instance in result.Response.Where(i => i.Checks.All(c => c.Status.Status == HealthStatus.Passing.Status)).Select(i => ConsulUtil.Create(i.Service)).Where(i => i != null))
                {
                    instances.Add(instance);
                }
                _instances.TryAdd(consulServiceName, instances);
            }).Wait();

            return instances.ToArray();
        }

        #endregion Implementation of IDiscoveryClient
    }
}