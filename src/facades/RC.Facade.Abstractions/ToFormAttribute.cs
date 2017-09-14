using System;

namespace Rabbit.Cloud.Facade.Abstractions
{
    public interface IBuilderMetadata { }
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public class ToFormAttribute : Attribute, IBuilderMetadata
    {
        public ToFormAttribute(string name = null)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}