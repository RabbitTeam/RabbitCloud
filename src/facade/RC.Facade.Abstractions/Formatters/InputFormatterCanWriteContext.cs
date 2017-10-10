using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Client.Abstractions;
using System;

namespace Rabbit.Cloud.Facade.Abstractions.Formatters
{
    public class InputFormatterCanWriteContext
    {
        public InputFormatterCanWriteContext(RabbitContext rabbitContext)
        {
            RabbitContext = rabbitContext;
        }

        public RabbitContext RabbitContext { get; }
        public virtual StringSegment ContentType { get; set; }
        public virtual object Object { get; protected set; }
        public virtual Type ObjectType { get; protected set; }
    }
}