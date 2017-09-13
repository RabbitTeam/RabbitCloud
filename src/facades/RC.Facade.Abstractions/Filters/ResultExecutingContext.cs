using RC.Discovery.Client.Abstractions;
using System.Collections.Generic;

namespace Rabbit.Cloud.Facade.Abstractions.Filters
{
    public class ResultExecutingContext : FilterContext
    {
        public ResultExecutingContext(RabbitContext rabbitContext, IList<IFilterMetadata> filters) : base(rabbitContext, filters)
        {
        }

        public virtual bool Cancel { get; set; }
    }
}