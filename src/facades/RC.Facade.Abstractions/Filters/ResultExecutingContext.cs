using RC.Discovery.Client.Abstractions;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Facade.Abstractions.Filters
{
    public class ResultExecutingContext : FilterContext
    {
        public ResultExecutingContext(RabbitContext rabbitContext, IList<IFilterMetadata> filters, Type returnType) : base(rabbitContext, filters)
        {
            ReturnType = returnType;
        }

        public Type ReturnType { get; }

        public virtual bool Cancel { get; set; }
    }
}