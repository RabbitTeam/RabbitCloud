using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitCloud.Registry.Abstractions
{
    public delegate void NotifyDelegate(ServiceRegistryDescriptor registryDescriptor, IReadOnlyCollection<ServiceRegistryDescriptor> descriptors);

    public interface IDiscoveryService
    {
        void Subscribe(ServiceRegistryDescriptor descriptor, NotifyDelegate listener);

        void UnSubscribe(ServiceRegistryDescriptor descriptor, NotifyDelegate listener);

        Task<IReadOnlyCollection<ServiceRegistryDescriptor>> Discover(ServiceRegistryDescriptor descriptor);
    }
}