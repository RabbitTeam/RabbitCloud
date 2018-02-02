using Microsoft.Extensions.Primitives;
using Rabbit.Go.Core.Internal.Descriptors;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Go.Internal
{
    public class MethodDescriptorCollectionProvider : IMethodDescriptorCollectionProvider
    {
        private readonly IMethodDescriptorProvider[] _methodDescriptorProviders;
        private readonly IMethodDescriptorChangeProvider[] _methodDescriptorChangeProviders;
        private IReadOnlyList<MethodDescriptor> _collection;

        public MethodDescriptorCollectionProvider(IEnumerable<IMethodDescriptorProvider> methodDescriptorProviders, IEnumerable<IMethodDescriptorChangeProvider> methodDescriptorChangeProviders)
        {
            _methodDescriptorProviders = methodDescriptorProviders.OrderBy(i => i.Order).ToArray();
            _methodDescriptorChangeProviders = methodDescriptorChangeProviders.ToArray();
            ChangeToken.OnChange(GetCompositeChangeToken, UpdateCollection);
        }

        #region Implementation of IMethodDescriptorCollectionProvider

        public IReadOnlyList<MethodDescriptor> Items
        {
            get
            {
                if (_collection == null)
                    UpdateCollection();
                return _collection;
            }
        }

        #endregion Implementation of IMethodDescriptorCollectionProvider

        private IChangeToken GetCompositeChangeToken()
        {
            if (_methodDescriptorChangeProviders.Length == 1)
            {
                return _methodDescriptorChangeProviders[0].GetChangeToken();
            }

            var changeTokens = new IChangeToken[_methodDescriptorChangeProviders.Length];
            for (var i = 0; i < _methodDescriptorChangeProviders.Length; i++)
            {
                changeTokens[i] = _methodDescriptorChangeProviders[i].GetChangeToken();
            }

            return new CompositeChangeToken(changeTokens);
        }

        private void UpdateCollection()
        {
            var providerContext = new MethodDescriptorProviderContext();

            foreach (var provider in _methodDescriptorProviders)
            {
                provider.OnProvidersExecuting(providerContext);
            }

            for (var i = _methodDescriptorProviders.Length - 1; i >= 0; i--)
            {
                _methodDescriptorProviders[i].OnProvidersExecuted(providerContext);
            }

            _collection = providerContext.Results.ToArray();
        }
    }
}