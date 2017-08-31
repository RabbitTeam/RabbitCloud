using Consul;
using RabbitCloud.Registry.Abstractions;
using System.Threading.Tasks;

namespace RabbitCloud.Registry.Consul
{
    public class ConsulRegistryService : IRegistryService<ConsulRegistration>
    {
        private readonly ConsulClient _consulClient;

        public ConsulRegistryService(ConsulClient consulClient)
        {
            _consulClient = consulClient;
        }

        #region Implementation of IRegistryService<in ConsulRegistration>

        public async Task RegisterAsync(ConsulRegistration registration)
        {
            await _consulClient.Agent.ServiceRegister(registration.AgentServiceRegistration);
        }

        public async Task DeregisterAsync(ConsulRegistration registration)
        {
            await _consulClient.Agent.ServiceDeregister(registration.InstanceId);
        }

        #endregion Implementation of IRegistryService<in ConsulRegistration>

        #region IDisposable

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _consulClient?.Dispose();
        }

        #endregion IDisposable
    }
}