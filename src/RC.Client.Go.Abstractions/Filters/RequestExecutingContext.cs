using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Go.Abstractions.Filters
{
    public class RequestExecutingContext : FilterContext
    {
        public RequestExecutingContext(GoRequestContext goRequestContext, IList<IFilterMetadata> filters, IDictionary<string, object> arguments)
            : base(goRequestContext, filters)
        {
            Arguments = arguments;
        }

        //        public IDictionary<string, object> Arguments { get; }
        public virtual object Result { get; set; }
    }
}