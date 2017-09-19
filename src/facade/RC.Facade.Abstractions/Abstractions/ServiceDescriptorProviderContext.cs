using System.Collections.Generic;

namespace Rabbit.Cloud.Facade.Abstractions
{
    public class ServiceDescriptorProviderContext
    {
        public IList<ServiceDescriptor> Results { get; } = new List<ServiceDescriptor>();
    }
}