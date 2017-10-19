using System;

namespace Rabbit.Cloud.Guise
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ToBodyAttribute : Attribute
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