using RabbitCloud.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitCloud.Registry.Abstractions
{
    public delegate void NotifyDelegate(ServiceKey serviceKey, IReadOnlyCollection<ServiceRegistryDescriptor> descriptors);

    public interface IDiscoveryService
    {
        void Subscribe(ServiceKey serviceKey, NotifyDelegate listener);

        void UnSubscribe(ServiceKey serviceKey, NotifyDelegate listener);

        Task<IReadOnlyCollection<ServiceRegistryDescriptor>> Discover(ServiceKey serviceKey);
    }
}