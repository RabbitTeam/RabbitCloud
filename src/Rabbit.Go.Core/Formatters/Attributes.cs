using System;

namespace Rabbit.Go.Formatters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Parameter)]
    public class CustomFormatterAttribute : Attribute
    {
        public CustomFormatterAttribute(Type formatterType)
        {
            if (!typeof(IKeyValueFormatter).IsAssignableFrom(formatterType))
                throw new InvalidOperationException($"{formatterType} not IKeyValueFormatter type.");
            FormatterType = formatterType;
        }

        public Type FormatterType { get; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class KeyNameAttribute : Attribute
    {
        public KeyNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyIgnoreAttribute : Attribute
    {
    }
}