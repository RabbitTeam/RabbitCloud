using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Discovery.Abstractions;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Discovery.Consul.Registry
{
    public class ConsulRegistryService : ConsulService, IRegistryService<ConsulRegistration>
    {
        private HeartbeatManager _heartbeatManager;

        #region Constructor

        public ConsulRegistryService(IOptionsMonitor<ConsulOptions> consulOptionsMonitor, ILoggerFactory loggerFactory) : base(consulOptionsMonitor)
        {
            var heartbeatManagerLogger = loggerFactory.CreateLogger<HeartbeatManager>();
            consulOptionsMonitor.OnChange(options =>
            {
                _heartbeatManager = new HeartbeatManager(ConsulClient, heartbeatManagerLogger);
            });
            _heartbeatManager = new HeartbeatManager(ConsulClient, heartbeatManagerLogger);
        }

        #endregion Constructor

        #region Implementation of IRegistryService<in ConsulRegistration>

        public async Task RegisterAsync(ConsulRegistration registration)
        {
            var serviceRegistration = registration.AgentServiceRegistration;

            await ConsulClient.Agent.ServiceRegister(serviceRegistration);

            if (serviceRegistration.Check?.TTL != null)
                await _heartbeatManager.AddHeartbeat(serviceRegistration.ID, serviceRegistration.Check.TTL.Value);
        }

        public async Task DeregisterAsync(ConsulRegistration registration)
        {
            _heartbeatManager.RemoveHeartbeat(registration.ServiceId);
            await ConsulClient.Agent.ServiceDeregister(registration.InstanceId);
        }

        #endregion Implementation of IRegistryService<in ConsulRegistration>
    }
}