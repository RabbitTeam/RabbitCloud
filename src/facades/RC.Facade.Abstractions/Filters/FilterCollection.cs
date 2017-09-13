using System;
using System.Collections.ObjectModel;

namespace Rabbit.Cloud.Facade.Abstractions.Filters
{
    public class FilterCollection : Collection<IFilterMetadata>
    {
        public IFilterMetadata Add<TFilterType>() where TFilterType : IFilterMetadata
        {
            return Add(typeof(TFilterType));
        }

        public IFilterMetadata Add(Type filterType)
        {
            if (filterType == null)
            {
                throw new ArgumentNullException(nameof(filterType));
            }

            return Add(filterType, 0);
        }

        public IFilterMetadata Add<TFilterType>(int order) where TFilterType : IFilterMetadata
        {
            return Add(typeof(TFilterType), order);
        }

        public IFilterMetadata Add(Type filterType, int order)
        {
            if (filterType == null)
            {
                throw new ArgumentNullException(nameof(filterType));
            }

            if (!typeof(IFilterMetadata).IsAssignableFrom(filterType))
            {
                var message =
                    $"FormatTypeMustDeriveFromType {filterType.FullName} , {typeof(IFilterMetadata).FullName}";
                throw new ArgumentException(message, nameof(filterType));
            }

            var filter = new TypeFilterAttribute(filterType) { Order = order };
            Add(filter);
            return filter;
        }

        public IFilterMetadata AddService<TFilterType>() where TFilterType : IFilterMetadata
        {
            return AddService(typeof(TFilterType));
        }

        public IFilterMetadata AddService(Type filterType)
        {
            if (filterType == null)
            {
                throw new ArgumentNullException(nameof(filterType));
            }

            return AddService(filterType, 0);
        }

        public IFilterMetadata AddService<TFilterType>(int order) where TFilterType : IFilterMetadata
        {
            return AddService(typeof(TFilterType), order);
        }

        public IFilterMetadata AddService(Type filterType, int order)
        {
            if (filterType == null)
            {
                throw new ArgumentNullException(nameof(filterType));
            }

            if (!typeof(IFilterMetadata).IsAssignableFrom(filterType))
            {
                var message =
                    $"FormatTypeMustDeriveFromType '{filterType.FullName}' '{typeof(IFilterMetadata).FullName}'";
                throw new ArgumentException(message, nameof(filterType));
            }

            var filter = new ServiceFilterAttribute(filterType) { Order = order };
            Add(filter);
            return filter;
        }
    }
}