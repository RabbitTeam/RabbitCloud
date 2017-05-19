using Consul;
using RabbitCloud.Abstractions;
using RabbitCloud.Registry.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitCloud.Registry.Consul
{
    public class ConsulRegistryTable : IRegistryTable
    {
        private readonly ConsulClient _consulClient;
        private readonly IList<ServiceRegistryDescriptor> _registeredServices = new List<ServiceRegistryDescriptor>();

        public ConsulRegistryTable(Uri url)
        {
            _consulClient = new ConsulClient(config =>
              {
                  config.Address = url;
              });
        }

        #region Implementation of IRegistryService

        public async Task RegisterAsync(ServiceRegistryDescriptor descriptor)
        {
            var agentServiceRegistration = new AgentServiceRegistration
            {
                Address = descriptor.Host,
                ID = $"{descriptor.Host}:{descriptor.Port}-{descriptor.ServiceKey.Name}",
                Name = "rabbitrpc_" + descriptor.ServiceKey.Group,
                Port = descriptor.Port,
                Check = new AgentServiceCheck
                {
                    TCP = "10s"
                },
                Tags = new[]
                {
                    "protocol_"+descriptor.Protocol
                }
            };
            var result = await _consulClient.Agent.ServiceRegister(agentServiceRegistration);

            _registeredServices.Add(descriptor);
        }

        public async Task UnRegisterAsync(ServiceRegistryDescriptor descriptor)
        {
            var result =
                await _consulClient.Agent.ServiceDeregister(
                    $"{descriptor.Host}:{descriptor.Port}-{descriptor.ServiceKey.Name}");
        }

        public Task SetAvailableAsync(ServiceRegistryDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        public Task SetUnAvailableAsync(ServiceRegistryDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<ServiceRegistryDescriptor> GetRegisteredServices()
        {
            return _registeredServices.ToArray();
        }

        #endregion Implementation of IRegistryService

        #region Implementation of IDiscoveryService

        public Task Subscribe(ServiceRegistryDescriptor descriptor, NotifyDelegate listener)
        {
            throw new NotImplementedException();
        }

        public Task UnSubscribe(ServiceRegistryDescriptor descriptor, NotifyDelegate listener)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyCollection<ServiceRegistryDescriptor>> Discover(ServiceRegistryDescriptor descriptor)
        {
            var result = await _consulClient.Agent.Services();
            var dict = result.Response;
            var services = dict.Values;

            return services.Where(i => i.Service.StartsWith("rabbitrpc_")).Select(i => new ServiceRegistryDescriptor
            {
                Host = i.Address,
                Port = i.Port,
                Protocol = i.Tags[0],
                ServiceKey = new ServiceKey(i.Service.Remove(0, "rabbitrpc_".Length),
                          i.ID.Substring(i.ID.IndexOf("-") + 1), "1.0.0")
            })
                .ToArray();
        }

        #endregion Implementation of IDiscoveryService
    }
}