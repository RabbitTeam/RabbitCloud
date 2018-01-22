using System.Collections.Generic;

namespace Rabbit.Go.Core.Internal
{
    public interface IMethodDescriptorCollectionProvider
    {
        IReadOnlyList<MethodDescriptor> Items { get; }
    }
}