using Rabbit.Cloud.Client.Abstractions;
using System.Collections.Generic;

namespace Rabbit.Cloud.Facade.Abstractions.Filters
{
    public class RequestExecutingContext : FilterContext
    {
        public RequestExecutingContext(RabbitContext rabbitContext, IList<IFilterMetadata> filters, IDictionary<string, object> arguments) : base(rabbitContext, filters)
        {
            Arguments = arguments;
        }

        public virtual IDictionary<string, object> Arguments { get; }
    }
}