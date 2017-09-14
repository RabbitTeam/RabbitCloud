using System;

namespace Rabbit.Cloud.Facade.Abstractions
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public class ToFormAttribute : Attribute, IBuilderMetadata
    {
        public ToFormAttribute()
        {
        }

        public ToFormAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}