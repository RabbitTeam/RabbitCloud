using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Go.Abstractions.Filters
{
    public abstract class FilterContext : GoRequestContext
    {
        protected FilterContext(GoRequestContext goRequestContext, IList<IFilterMetadata> filters)
            : base(goRequestContext)
        {
            Filters = filters;
        }

        public virtual IList<IFilterMetadata> Filters { get; }
    }
}