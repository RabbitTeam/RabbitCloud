using System.Collections.Generic;

namespace Rabbit.Go.Internal
{
    public interface IMethodDescriptorCollectionProvider
    {
        IReadOnlyList<MethodDescriptor> Items { get; }
    }
}