using Consul;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Discovery.Abstractions;
using Rabbit.Cloud.Extensions.Consul.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Extensions.Consul.Discovery
{
    public class ConsulDiscoveryClient : ConsulService, IDiscoveryClient
    {
        private readonly ConcurrentDictionary<string, IReadOnlyCollection<IServiceInstance>> _instances = new ConcurrentDictionary<string, IReadOnlyCollection<IServiceInstance>>(StringComparer.OrdinalIgnoreCase);

        #region Constructor

        public ConsulDiscoveryClient(ConsulClient consulClient) : base(consulClient)
        {
            //first load
            LoadServices().Wait();

            Task.Factory.StartNew(async () =>
            {
                var response = await consulClient.Catalog.Services();
                var index = response.LastIndex;

                //watcher
                while (!Disposed)
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

        public ConsulDiscoveryClient(IOptionsMonitor<RabbitConsulOptions> consulOptionsMonitor)
            : this(consulOptionsMonitor.CurrentValue.CreateClient())
        {
        }

        #endregion Constructor

        #region Implementation of IDiscoveryClient

        public string Description => "Rabbit Cloud Consul Client";
        public IReadOnlyCollection<string> Services { get; private set; }

        public IReadOnlyCollection<IServiceInstance> GetInstances(string serviceId)
        {
            _instances.TryGetValue(serviceId, out var instances);
            return instances;
        }

        #endregion Implementation of IDiscoveryClient

        #region Private Method

        private async Task LoadServices()
        {
            var agentEndpoint = ConsulClient.Agent;

            var result = await agentEndpoint.Services();
            var services = result.Response.Values;
            var instances = services.Select(ConsulUtil.Create).Where(i => i != null).ToArray();

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