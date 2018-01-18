using System.Collections.Generic;

namespace Rabbit.Go.Abstractions.Filters
{
    public class ResultExecutingContext : FilterContext
    {
        public ResultExecutingContext(RequestContext requestContext, IList<IFilterMetadata> filters, object result) : base(requestContext, filters)
        {
            Result = result;
        }

        public object Result { get; set; }
        public virtual bool Cancel { get; set; }
    }
}