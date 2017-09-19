using RC.Abstractions;
using System.Collections.Generic;

namespace Rabbit.Cloud.Facade.Abstractions.Filters
{
    public abstract class FilterContext
    {
        protected FilterContext(RabbitContext rabbitContext, IList<IFilterMetadata> filters)
        {
            RabbitContext = rabbitContext;
            Filters = filters;
        }

        public RabbitContext RabbitContext { get; set; }

        public virtual IList<IFilterMetadata> Filters { get; }
    }
}