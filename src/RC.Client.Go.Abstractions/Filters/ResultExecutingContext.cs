using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Go.Abstractions.Filters
{
    public class ResultExecutingContext : FilterContext
    {
        public ResultExecutingContext(GoRequestContext goRequestContext, IList<IFilterMetadata> filters, object result) : base(goRequestContext, filters)
        {
            Result = result;
        }

        public object Result { get; set; }
        public virtual bool Cancel { get; set; }
    }
}