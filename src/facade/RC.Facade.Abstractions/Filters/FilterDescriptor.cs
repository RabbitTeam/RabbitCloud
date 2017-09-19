using System;

namespace Rabbit.Cloud.Facade.Abstractions.Filters
{
    public class FilterDescriptor
    {
        public FilterDescriptor(IFilterMetadata filter)
        {
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));

            var orderedFilter = filter as IOrderedFilter;
            if (orderedFilter != null)
                Order = orderedFilter.Order;
        }

        public IFilterMetadata Filter { get; }
        public int Order { get; set; }
    }
}