using System;

namespace Rabbit.Cloud.Facade.Abstractions.ModelBinding
{
    public interface IBinderTypeProviderMetadata
    {
        Type BinderType { get; }
    }
}