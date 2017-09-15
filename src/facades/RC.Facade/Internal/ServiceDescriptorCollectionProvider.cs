using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.Abstractions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace Rabbit.Cloud.Facade.Internal
{
    public interface IServiceDescriptorCollectionProvider
    {
        ServiceDescriptorCollection ServiceDescriptors { get; }
    }

    public class ServiceDescriptorCollectionProvider : IServiceDescriptorCollectionProvider
    {
        private readonly IServiceDescriptorProvider[] _serviceDescriptorProviders;
        private ServiceDescriptorCollection _collection;
        private int _version = -1;

        public ServiceDescriptorCollectionProvider(IEnumerable<IServiceDescriptorProvider> serviceDescriptorProviders)
        {
            _serviceDescriptorProviders = serviceDescriptorProviders.OrderBy(i => i.Order).ToArray();
        }

        #region Implementation of IServiceDescriptorCollectionProvider

        public ServiceDescriptorCollection ServiceDescriptors
        {
            get
            {
                if (_collection == null)
                {
                    UpdateCollection();
                }

                return _collection;
            }
        }

        #endregion Implementation of IServiceDescriptorCollectionProvider

        #region Private Method

        private void UpdateCollection()
        {
            var context = new ServiceDescriptorProviderContext();

            foreach (var provider in _serviceDescriptorProviders)
            {
                provider.OnProvidersExecuting(context);
            }

            for (var i = _serviceDescriptorProviders.Length - 1; i >= 0; i--)
            {
                _serviceDescriptorProviders[i].OnProvidersExecuted(context);
            }

            _collection = new ServiceDescriptorCollection(
                new ReadOnlyCollection<ServiceDescriptor>(context.Results),
                Interlocked.Increment(ref _version));
        }

        #endregion Private Method
    }
}