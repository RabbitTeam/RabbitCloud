using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Go.Abstractions.Filters
{
    public class FilterProviderContext
    {
        public FilterProviderContext(GoRequestContext requestContext, IList<FilterItem> items)
        {
            RequestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
            Results = items ?? throw new ArgumentNullException(nameof(items));
        }

        public GoRequestContext RequestContext { get; set; }

        public IList<FilterItem> Results { get; set; }
    }
}