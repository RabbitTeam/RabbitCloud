using Rabbit.Cloud.Client.Go.Abstractions.Filters;
using Rabbit.Cloud.Client.Go.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Client.Go.Internal
{
    public struct FilterFactoryResult
    {
        public FilterFactoryResult(
            FilterItem[] cacheableFilters,
            IFilterMetadata[] filters)
        {
            CacheableFilters = cacheableFilters;
            Filters = filters;
        }

        public FilterItem[] CacheableFilters { get; }

        public IFilterMetadata[] Filters { get; }
    }

    public class FilterFactory
    {
        public static IReadOnlyList<IFilterMetadata> GetAllFilters(
            ServiceInvokerContext invokerContext)
        {
            return GetAllFilters(new IFilterProvider[] { new DefaultFilterProvider() }, invokerContext);
        }

        public static IReadOnlyList<IFilterMetadata> GetAllFilters(IEnumerable<IFilterProvider> filterProviders, ServiceInvokerContext invokerContext)
        {
            if (filterProviders == null)
            {
                throw new ArgumentNullException(nameof(filterProviders));
            }

            if (invokerContext == null)
            {
                throw new ArgumentNullException(nameof(invokerContext));
            }
            var requestModel = invokerContext.RequestModel;

            var filterItems = requestModel
                .GetRequestAttributes()
                .OfType<IFilterMetadata>()
                .OrderBy(f =>
                {
                    if (f is IOrderedFilter orderedFilter)
                        return orderedFilter.Order;

                    return 20;
                })
                .Select(i => new FilterItem(new FilterDescriptor(i, 0)))
                .ToList();

            var providerContext = new FilterProviderContext(invokerContext.RequestContext, filterItems);

            foreach (var filterProvider in filterProviders)
            {
                filterProvider.OnProvidersExecuting(providerContext);
            }

            return providerContext.Results.Select(i => i.Filter).ToArray();
        }
    }
}