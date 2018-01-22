using System.Collections.Generic;

namespace Rabbit.Go.Core
{
    public class MethodDescriptorProviderContext
    {
        public IList<MethodDescriptor> Results { get; } = new List<MethodDescriptor>();
    }

    public interface IMethodDescriptorProvider
    {
        int Order { get; }

        void OnProvidersExecuting(MethodDescriptorProviderContext context);

        void OnProvidersExecuted(MethodDescriptorProviderContext context);
    }
}