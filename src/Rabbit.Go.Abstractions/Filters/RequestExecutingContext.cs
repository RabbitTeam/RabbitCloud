using System.Collections.Generic;

namespace Rabbit.Go.Abstractions.Filters
{
    public class RequestExecutingContext : FilterContext
    {
        public RequestExecutingContext(RequestContext requestContext, IList<IFilterMetadata> filters, IDictionary<string, object> arguments)
            : base(requestContext, filters)
        {
            Arguments = arguments;
        }

        //        public IDictionary<string, object> Arguments { get; }
        public virtual object Result { get; set; }
    }
}