using System;

namespace Rabbit.Cloud.Guise
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true)]
    public class ToQueryAttribute : Attribute
    {
        public ToQueryAttribute()
        {
        }

        public ToQueryAttribute(string name)
        {
            Name = name;
        }

        public ToQueryAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public object Value { get; set; }
    }
}