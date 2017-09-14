using Consul;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Registry.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Extensions.Consul.Registry
{
    public class ConsulRegistryService : ConsulService, IRegistryService<ConsulRegistration>
    {
        private readonly ILogger<ConsulRegistryService> _logger;

        #region Constructor

        public ConsulRegistryService(ConsulClient consulClient, ILogger<ConsulRegistryService> logger) : base(consulClient)
        {
            _logger = logger;
        }

        public ConsulRegistryService(IOptionsMonitor<RabbitConsulOptions> consulOptionsMonitor) : base(consulOptionsMonitor)
        {
        }

        #endregion Constructor

        #region Implementation of IRegistryService<in ConsulRegistration>

        public async Task RegisterAsync(ConsulRegistration registration)
        {
            var serviceRegistration = registration.AgentServiceRegistration;

            await ClearHealthCheck(serviceRegistration.Name);

            await ConsulClient.Agent.ServiceRegister(serviceRegistration);
        }

        public async Task DeregisterAsync(ConsulRegistration registration)
        {
            await ConsulClient.Agent.ServiceDeregister(registration.InstanceId);
        }

        #endregion Implementation of IRegistryService<in ConsulRegistration>

        #region Private Method

        private async Task ClearHealthCheck(string serviceName)
        {
            try
            {
                var checkResult = await ConsulClient.Health.Checks(serviceName);
                if (checkResult.Response != null && checkResult.Response.Any())
                {
                    foreach (var healthCheck in checkResult.Response)
                    {
                        await ConsulClient.Agent.CheckDeregister(healthCheck.CheckID);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "ClearHealthCheck failure.");
            }
        }

        #endregion Private Method
    }
}