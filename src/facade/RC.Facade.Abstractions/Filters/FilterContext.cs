using Rabbit.Cloud.Client.Abstractions;
using System.Collections.Generic;

namespace Rabbit.Cloud.Facade.Abstractions.Filters
{
    public abstract class FilterContext
    {
        protected FilterContext(IRabbitContext rabbitContext, IList<IFilterMetadata> filters)
        {
            RabbitContext = rabbitContext;
            Filters = filters;
        }

        public IRabbitContext RabbitContext { get; set; }

        public virtual IList<IFilterMetadata> Filters { get; }
    }
}