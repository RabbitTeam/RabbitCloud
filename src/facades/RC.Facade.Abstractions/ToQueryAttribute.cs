using System;

namespace Rabbit.Cloud.Facade.Abstractions
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true)]
    public class ToQueryAttribute : Attribute, IBuilderMetadata
    {
        public ToQueryAttribute(string name = null, string value = null)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}