using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Go.Core.Internal
{
    public class MethodDescriptorCollectionProvider : IMethodDescriptorCollectionProvider
    {
        public MethodDescriptorCollectionProvider(IEnumerable<IMethodDescriptorProvider> providers)
        {
            var providerArray = providers.OrderBy(i => i.Order).ToArray();
            var providerContext = new MethodDescriptorProviderContext();

            foreach (var provider in providerArray)
            {
                provider.OnProvidersExecuting(providerContext);
            }

            for (var i = providerArray.Length - 1; i >= 0; i--)
            {
                providerArray[i].OnProvidersExecuted(providerContext);
            }

            Items = providerContext.Results.ToArray();
        }

        #region Implementation of IMethodDescriptorCollectionProvider

        public IReadOnlyList<MethodDescriptor> Items { get; }

        #endregion Implementation of IMethodDescriptorCollectionProvider
    }
}