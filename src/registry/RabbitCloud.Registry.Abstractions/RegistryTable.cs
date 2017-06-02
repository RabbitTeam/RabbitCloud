using RabbitCloud.Abstractions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitCloud.Registry.Abstractions
{
    public abstract class RegistryTable : IRegistryTable
    {
        private readonly IList<ServiceRegistryDescriptor> _registeredServices = new List<ServiceRegistryDescriptor>();
        private readonly ConcurrentDictionary<ServiceKey, NotifyDelegate> _notifyDelegates = new ConcurrentDictionary<ServiceKey, NotifyDelegate>();

        protected abstract Task DoRegisterAsync(ServiceRegistryDescriptor descriptor);

        protected abstract Task DoUnRegisterAsync(ServiceRegistryDescriptor descriptor);

        protected abstract Task<IEnumerable<ServiceRegistryDescriptor>> DoDiscover(ServiceKey serviceKey);

        #region Implementation of IRegistryService

        public virtual async Task RegisterAsync(ServiceRegistryDescriptor descriptor)
        {
            await DoRegisterAsync(descriptor);
            _registeredServices.Add(descriptor);
        }

        public virtual async Task UnRegisterAsync(ServiceRegistryDescriptor descriptor)
        {
            await DoUnRegisterAsync(descriptor);
            _registeredServices.Remove(descriptor);
        }

        public virtual IReadOnlyCollection<ServiceRegistryDescriptor> GetRegisteredServices()
        {
            return _registeredServices.ToArray();
        }

        #endregion Implementation of IRegistryService

        #region Implementation of IDiscoveryService

        public virtual void Subscribe(ServiceKey serviceKey, NotifyDelegate listener)
        {
            Task.Run(async () => await Discover(serviceKey)).Wait();

            if (_notifyDelegates.TryGetValue(serviceKey, out NotifyDelegate value))
                listener = value + listener;

            _notifyDelegates.AddOrUpdate(serviceKey, listener, (s, ss) => listener);
        }

        public virtual void UnSubscribe(ServiceKey serviceKey, NotifyDelegate listener)
        {
            if (_notifyDelegates.TryGetValue(serviceKey, out NotifyDelegate value))
                listener = value - listener;

            _notifyDelegates.TryUpdate(serviceKey, listener, value);
        }

        public virtual async Task<IReadOnlyCollection<ServiceRegistryDescriptor>> Discover(ServiceKey serviceKey)
        {
            var descriptors = await DoDiscover(serviceKey);
            return descriptors.ToArray();
        }

        #endregion Implementation of IDiscoveryService
    }
}