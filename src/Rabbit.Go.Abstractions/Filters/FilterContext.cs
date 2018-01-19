using System.Collections.Generic;

namespace Rabbit.Go.Abstractions.Filters
{
    public abstract class FilterContext : RequestContext
    {
        protected FilterContext(RequestContext requestContext, IList<IFilterMetadata> filters)
            : base(requestContext)
        {
            Filters = filters;
        }

        public virtual IList<IFilterMetadata> Filters { get; }
    }
}