using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Discovery.Abstractions;
using Rabbit.Cloud.Discovery.Consul.Internal;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Discovery.Consul.Registry
{
    public class ConsulRegistryService : ConsulService, IRegistryService<ConsulRegistration>
    {
        private readonly ServiceNameResolver _serviceNameResolver;
        private HeartbeatManager _heartbeatManager;

        #region Constructor

        public ConsulRegistryService(IOptionsMonitor<ConsulOptions> consulOptionsMonitor, ILoggerFactory loggerFactory, ServiceNameResolver serviceNameResolver) : base(consulOptionsMonitor)
        {
            _serviceNameResolver = serviceNameResolver;
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
            serviceRegistration.Name = _serviceNameResolver.GetConsulNameByLocalName(serviceRegistration.Name);

            await ConsulClient.Agent.ServiceRegister(serviceRegistration);

            if (serviceRegistration.Check?.TTL != null)
                await _heartbeatManager.AddHeartbeat(serviceRegistration.ID, serviceRegistration.Check.TTL.Value);
        }

        public async Task DeregisterAsync(ConsulRegistration registration)
        {
            var serviceRegistration = registration.AgentServiceRegistration;
            serviceRegistration.Name = _serviceNameResolver.GetConsulNameByLocalName(serviceRegistration.Name);

            _heartbeatManager.RemoveHeartbeat(registration.ServiceId);
            await ConsulClient.Agent.ServiceDeregister(registration.InstanceId);
        }

        #endregion Implementation of IRegistryService<in ConsulRegistration>
    }
}