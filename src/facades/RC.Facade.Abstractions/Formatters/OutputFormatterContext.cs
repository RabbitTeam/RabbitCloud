using RC.Discovery.Client.Abstractions;
using System;
using System.IO;

namespace Rabbit.Cloud.Facade.Abstractions.Formatters
{
    public class OutputFormatterContext
    {
        public OutputFormatterContext(RabbitContext rabbitContext, Type modelType, Stream stream, bool treatEmptyInputAsDefaultValue = false)
        {
            RabbitContext = rabbitContext;
            ModelType = modelType;
            Stream = stream;
            TreatEmptyInputAsDefaultValue = treatEmptyInputAsDefaultValue;
        }

        public bool TreatEmptyInputAsDefaultValue { get; }
        public RabbitContext RabbitContext { get; }
        public Type ModelType { get; }
        public Stream Stream { get; }
    }
}