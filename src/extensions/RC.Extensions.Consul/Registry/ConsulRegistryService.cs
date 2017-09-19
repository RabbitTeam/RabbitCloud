using Consul;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Registry.Abstractions;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Extensions.Consul.Registry
{
    public class ConsulRegistryService : ConsulService, IRegistryService<ConsulRegistration>
    {
        private readonly HeartbeatManager _heartbeatManager;

        #region Constructor

        public ConsulRegistryService(IConsulClient consulClient, ILoggerFactory loggerFactory) : base(consulClient)
        {
            _heartbeatManager = new HeartbeatManager(ConsulClient, loggerFactory.CreateLogger<HeartbeatManager>());
        }

        public ConsulRegistryService(IOptionsMonitor<RabbitConsulOptions> consulOptionsMonitor, ILoggerFactory loggerFactory) : base(consulOptionsMonitor)
        {
            _heartbeatManager = new HeartbeatManager(ConsulClient, loggerFactory.CreateLogger<HeartbeatManager>());
        }

        #endregion Constructor

        #region Implementation of IRegistryService<in ConsulRegistration>

        public async Task RegisterAsync(ConsulRegistration registration)
        {
            var serviceRegistration = registration.AgentServiceRegistration;

            if (serviceRegistration.Check.TTL.HasValue)
                await _heartbeatManager.AddHeartbeat(serviceRegistration.ID, serviceRegistration.Check.TTL.Value);

            await ConsulClient.Agent.ServiceRegister(serviceRegistration);
        }

        public async Task DeregisterAsync(ConsulRegistration registration)
        {
            _heartbeatManager.RemoveHeartbeat(registration.ServiceId);
            await ConsulClient.Agent.ServiceDeregister(registration.InstanceId);
        }

        #endregion Implementation of IRegistryService<in ConsulRegistration>
    }
}