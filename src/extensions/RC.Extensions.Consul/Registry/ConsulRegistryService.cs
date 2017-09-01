using Consul;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Registry.Abstractions;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Extensions.Consul.Registry
{
    public class ConsulRegistryService : ConsulService, IRegistryService<ConsulRegistration>
    {
        #region Constructor

        public ConsulRegistryService(ConsulClient consulClient) : base(consulClient)
        {
        }

        public ConsulRegistryService(IOptionsMonitor<RabbitConsulOptions> consulOptionsMonitor) : base(consulOptionsMonitor)
        {
        }

        #endregion Constructor

        #region Implementation of IRegistryService<in ConsulRegistration>

        public async Task RegisterAsync(ConsulRegistration registration)
        {
            await ConsulClient.Agent.ServiceRegister(registration.AgentServiceRegistration);
        }

        public async Task DeregisterAsync(ConsulRegistration registration)
        {
            await ConsulClient.Agent.ServiceDeregister(registration.InstanceId);
        }

        #endregion Implementation of IRegistryService<in ConsulRegistration>
    }
}