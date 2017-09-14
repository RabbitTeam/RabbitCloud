using System;

namespace Rabbit.Cloud.Facade.Abstractions
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true)]
    public class ToHeaderAttribute : Attribute, IBuilderMetadata
    {
        public ToHeaderAttribute(string name = null, string value = null)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}