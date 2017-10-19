using System;

namespace Rabbit.Cloud.Guise
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true)]
    public class ToHeaderAttribute : Attribute
    {
        public ToHeaderAttribute()
        {
        }

        public ToHeaderAttribute(string name)
        {
            Name = name;
        }

        public ToHeaderAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public object Value { get; set; }
    }
}