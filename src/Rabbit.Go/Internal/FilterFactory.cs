using Rabbit.Go.Abstractions;
using Rabbit.Go.Abstractions.Filters;
using Rabbit.Go.ApplicationModels;
using Rabbit.Go.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Go.Internal
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
        public static IReadOnlyList<IFilterMetadata> GetAllFilters(RequestContext requestContext, RequestModel requestModel)
        {
            return GetAllFilters(new IFilterProvider[] { new DefaultFilterProvider() }, requestContext, requestModel);
        }

        public static IReadOnlyList<IFilterMetadata> GetAllFilters(IEnumerable<IFilterProvider> filterProviders, RequestContext requestContext, RequestModel requestModel)
        {
            if (filterProviders == null)
            {
                throw new ArgumentNullException(nameof(filterProviders));
            }

            if (requestContext == null)
            {
                throw new ArgumentNullException(nameof(requestContext));
            }
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

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

            var providerContext = new FilterProviderContext(requestContext, filterItems);

            foreach (var filterProvider in filterProviders)
            {
                filterProvider.OnProvidersExecuting(providerContext);
            }

            return providerContext.Results.Select(i => i.Filter).ToArray();
        }
    }
}