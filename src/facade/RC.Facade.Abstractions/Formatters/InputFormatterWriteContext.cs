using Rabbit.Cloud.Client.Abstractions;
using System;

namespace Rabbit.Cloud.Facade.Abstractions.Formatters
{
    public class InputFormatterWriteContext : InputFormatterCanWriteContext
    {
        public InputFormatterWriteContext(IRabbitContext rabbitContext, Type objectType, object @object)
            : base(rabbitContext)
        {
            ObjectType = objectType;
            Object = @object;
        }
    }
}