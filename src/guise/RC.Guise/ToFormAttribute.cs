using System;

namespace Rabbit.Cloud.Guise
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public class ToFormAttribute : Attribute
    {
        public ToFormAttribute()
        {
        }

        public ToFormAttribute(string name)
        {
            Name = name;
        }

        public ToFormAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public object Value { get; }
    }
}