using System;

namespace Rabbit.Cloud.Facade.Abstractions
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ToBodyAttribute : Attribute, IBuilderMetadata
    {
        public ToBodyAttribute()
        {
        }

        public ToBodyAttribute(string formatter)
        {
            Formatter = formatter;
        }

        public string Formatter { get; set; }
    }
}