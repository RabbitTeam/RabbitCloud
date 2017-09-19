using System;

namespace Rabbit.Cloud.Facade.Abstractions.MessageBuilding
{
    public interface IBuildingTypeProviderMetadata
    {
        Type BuildingType { get; }
    }
}