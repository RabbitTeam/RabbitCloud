using Rabbit.Cloud.Facade.Abstractions.ModelBinding;
using System;

namespace Rabbit.Cloud.Facade.Abstractions
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public class ToFormAttribute : Attribute, IBindingSourceMetadata, IModelNameProvider, IDefaultValueProviderMetadata
    {
        public ToFormAttribute()
        {
        }

        public ToFormAttribute(string name, string value = null)
        {
            Name = name;
            Value = value;
        }

        #region Implementation of IBindingSourceMetadata

        public BindingSource BindingSource { get; } = BindingSource.Form;

        #endregion Implementation of IBindingSourceMetadata

        #region Implementation of IModelNameProvider

        public string Name { get; }

        #endregion Implementation of IModelNameProvider

        #region Implementation of IDefaultValueProviderMetadata

        public object Value { get; }

        #endregion Implementation of IDefaultValueProviderMetadata
    }
}