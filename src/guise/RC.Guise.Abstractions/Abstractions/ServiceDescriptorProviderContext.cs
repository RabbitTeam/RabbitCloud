using System.Collections.Generic;

namespace Rabbit.Cloud.Guise.Abstractions
{
    public class ServiceDescriptorProviderContext
    {
        public IList<ServiceDescriptor> Results { get; } = new List<ServiceDescriptor>();
    }
}