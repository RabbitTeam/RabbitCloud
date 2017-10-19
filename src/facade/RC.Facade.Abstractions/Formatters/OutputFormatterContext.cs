using Rabbit.Cloud.Client.Abstractions;
using System;
using System.IO;

namespace Rabbit.Cloud.Facade.Abstractions.Formatters
{
    public class OutputFormatterContext
    {
        public OutputFormatterContext(IRabbitContext rabbitContext, Type modelType, Stream stream, bool treatEmptyInputAsDefaultValue = false)
        {
            RabbitContext = rabbitContext;
            ModelType = modelType;
            Stream = stream;
            TreatEmptyInputAsDefaultValue = treatEmptyInputAsDefaultValue;
        }

        public bool TreatEmptyInputAsDefaultValue { get; }
        public IRabbitContext RabbitContext { get; }
        public Type ModelType { get; }
        public Stream Stream { get; }
    }
}