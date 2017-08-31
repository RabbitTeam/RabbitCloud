using Consul;
using Rabbit.Discovery.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Discovery.Consul.Client
{
    public class ConsulDiscoveryClient : IDiscoveryClient
    {
        private readonly ConsulClient _consulClient;
        private bool _disposed;

        private readonly ConcurrentDictionary<string, IReadOnlyCollection<IServiceInstance>> _instances = new ConcurrentDictionary<string, IReadOnlyCollection<IServiceInstance>>(StringComparer.OrdinalIgnoreCase);

        public ConsulDiscoveryClient(ConsulClient consulClient)
        {
            _consulClient = consulClient;

            Task.Factory.StartNew(async () =>
            {
                //first load
                await LoadServices();

                var response = await consulClient.Catalog.Services();
                var index = response.LastIndex;

                //watcher
                while (!_disposed)
                {
                    response = await consulClient.Catalog.Services(new QueryOptions { WaitIndex = index });

                    //timeout ignore
                    if (response.LastIndex == index)
                        continue;

                    //reload
                    await LoadServices();

                    index = response.LastIndex;
                }
            });
        }

        #region Implementation of IDiscoveryClient

        public string Description => "Rabbit Cloud Consul Client";
        public IReadOnlyCollection<string> Services { get; private set; }

        public IReadOnlyCollection<IServiceInstance> GetInstances(string serviceId)
        {
            _instances.TryGetValue(serviceId, out var instances);
            return instances;
        }

        #endregion Implementation of IDiscoveryClient

        #region IDisposable

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _disposed = true;
            _consulClient?.Dispose();
        }

        #endregion IDisposable

        #region Private Method

        private async Task LoadServices()
        {
            var agentEndpoint = _consulClient.Agent;

            var result = await agentEndpoint.Services();
            var services = result.Response.Values;
            var instances = services.Select(ConsulServiceInstance.Create).ToArray();

            foreach (var instanceGroup in instances.GroupBy(i => i.ServiceId))
            {
                var serviceName = instanceGroup.Key;
                var serviceInstances = instanceGroup.ToArray();

                _instances.AddOrUpdate(serviceName, serviceInstances, (key, existing) => serviceInstances);
            }

            Services = instances.Select(i => i.ServiceId).ToArray();
        }

        #endregion Private Method
    }
}