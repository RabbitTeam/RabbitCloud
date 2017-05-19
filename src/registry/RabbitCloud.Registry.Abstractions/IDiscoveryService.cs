using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitCloud.Registry.Abstractions
{
    public delegate void NotifyDelegate(ServiceRegistryDescriptor registryDescriptor, ServiceRegistryDescriptor[] descriptors);

    public interface IDiscoveryService
    {
        Task Subscribe(ServiceRegistryDescriptor descriptor, NotifyDelegate listener);

        Task UnSubscribe(ServiceRegistryDescriptor descriptor, NotifyDelegate listener);

        Task<IReadOnlyCollection<ServiceRegistryDescriptor>> Discover(ServiceRegistryDescriptor descriptor);
    }
}