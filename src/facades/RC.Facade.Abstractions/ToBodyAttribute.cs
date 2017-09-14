using System;

namespace Rabbit.Cloud.Facade.Abstractions
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ToBodyAttribute : Attribute, IBuilderMetadata
    {
        public ToBodyAttribute(string formatter = null)
        {
            Formatter = formatter;
        }

        public string Formatter { get; set; }
    }
}