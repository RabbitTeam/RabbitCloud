using System;
using System.Threading.Tasks;

namespace Rabbit.Go.Formatters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Parameter)]
    public abstract class CustomFormatterAttribute : Attribute, IKeyValueFormatter
    {
        #region Implementation of IKeyValueFormatter

        public abstract Task FormatAsync(KeyValueFormatterContext context);

        #endregion Implementation of IKeyValueFormatter
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