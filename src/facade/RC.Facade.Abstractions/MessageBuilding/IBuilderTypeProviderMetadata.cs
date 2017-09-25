using System;

namespace Rabbit.Cloud.Facade.Abstractions.MessageBuilding
{
    public interface IBuilderTypeProviderMetadata
    {
        Type BuilderType { get; }
    }
}