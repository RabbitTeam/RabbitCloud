using Microsoft.Extensions.Primitives;

namespace Rabbit.Go.Core.Internal.Descriptors
{
    public interface IMethodDescriptorChangeProvider
    {
        IChangeToken GetChangeToken();
    }
}