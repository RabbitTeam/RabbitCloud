using System.Collections.Generic;

namespace Rabbit.Go.Abstractions
{
    public class MethodDescriptorProviderContext
    {
        public IList<MethodDescriptor> Results { get; } = new List<MethodDescriptor>();
    }
}