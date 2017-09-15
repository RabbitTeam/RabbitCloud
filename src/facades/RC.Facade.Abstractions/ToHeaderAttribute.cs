using Rabbit.Cloud.Facade.Abstractions.ModelBinding;
using System;

namespace Rabbit.Cloud.Facade.Abstractions
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true)]
    public class ToHeaderAttribute : Attribute, IBindingSourceMetadata, IModelNameProvider, IDefaultValueProviderMetadata
    {
        public ToHeaderAttribute()
        {
        }

        public ToHeaderAttribute(string name, string value = null)
        {
            Name = name;
            Value = value;
        }

        #region Implementation of IBindingSourceMetadata

        public BindingSource BindingSource { get; } = BindingSource.Header;

        #endregion Implementation of IBindingSourceMetadata

        #region Implementation of IModelNameProvider

        public string Name { get; }

        #endregion Implementation of IModelNameProvider

        #region Implementation of IDefaultValueProviderMetadata

        public object Value { get; set; }

        #endregion Implementation of IDefaultValueProviderMetadata
    }
}