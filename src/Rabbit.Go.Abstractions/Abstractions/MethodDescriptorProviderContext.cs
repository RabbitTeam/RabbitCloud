using System.Collections.Generic;

namespace Rabbit.Go
{
    public class MethodDescriptorProviderContext
    {
        public IList<MethodDescriptor> Results { get; } = new List<MethodDescriptor>();
    }
}