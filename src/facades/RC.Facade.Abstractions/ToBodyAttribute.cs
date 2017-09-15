using Rabbit.Cloud.Facade.Abstractions.ModelBinding;
using System;

namespace Rabbit.Cloud.Facade.Abstractions
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ToBodyAttribute : Attribute, IBindingSourceMetadata
    {
        public ToBodyAttribute()
        {
        }

        public ToBodyAttribute(string formatter)
        {
            Formatter = formatter;
        }

        public string Formatter { get; set; }

        #region Implementation of IBindingSourceMetadata

        public BindingSource BindingSource { get; } = BindingSource.Body;

        #endregion Implementation of IBindingSourceMetadata
    }
}