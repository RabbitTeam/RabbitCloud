using System;

namespace Rabbit.Cloud.Client.Go.Abstractions.Filters
{
    public class FilterDescriptor
    {
        public FilterDescriptor(IFilterMetadata filter, int filterScope)
        {
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));
            Scope = filterScope;

            if (Filter is IOrderedFilter orderedFilter)
            {
                Order = orderedFilter.Order;
            }
        }

        public IFilterMetadata Filter { get; }

        public int Order { get; set; }

        public int Scope { get; }
    }
}