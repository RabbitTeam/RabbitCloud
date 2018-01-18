using Rabbit.Go.Abstractions.Filters;
using System;

namespace Rabbit.Go.Internal
{
    public class DefaultFilterProvider : IFilterProvider
    {
        #region Implementation of IFilterProvider

        public int Order => -1000;

        public void OnProvidersExecuting(FilterProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var filter in context.Results)
            {
                ProvideFilter(context, filter);
            }
        }

        public void OnProvidersExecuted(FilterProviderContext context)
        {
        }

        #endregion Implementation of IFilterProvider

        public virtual void ProvideFilter(FilterProviderContext context, FilterItem filterItem)
        {
            if (filterItem.Filter != null)
            {
                return;
            }

            var filter = filterItem.Descriptor.Filter;

            if (!(filter is IFilterFactory filterFactory))
            {
                filterItem.Filter = filter;
                filterItem.IsReusable = true;
            }
            else
            {
                var services = context.RequestContext.RabbitContext.RequestServices;
                filterItem.Filter = filterFactory.CreateInstance(services);
                filterItem.IsReusable = filterFactory.IsReusable;

                if (filterItem.Filter == null)
                {
                    throw new InvalidOperationException("FormatTypeMethodMustReturnNotNullValue");
                }

                ApplyFilterToContainer(filterItem.Filter, filterFactory);
            }
        }

        private static void ApplyFilterToContainer(object actualFilter, IFilterMetadata filterMetadata)
        {
            if (actualFilter is IFilterContainer container)
            {
                container.FilterDefinition = filterMetadata;
            }
        }
    }
}