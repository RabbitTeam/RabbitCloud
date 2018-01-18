using System;
using System.Collections.Generic;

namespace Rabbit.Go.Abstractions.Filters
{
    public class FilterProviderContext
    {
        public FilterProviderContext(RequestContext requestContext, IList<FilterItem> items)
        {
            RequestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
            Results = items ?? throw new ArgumentNullException(nameof(items));
        }

        public RequestContext RequestContext { get; set; }

        public IList<FilterItem> Results { get; set; }
    }
}