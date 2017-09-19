using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Facade.Abstractions.MessageBuilding
{
    public class BuildingInfo
    {
        public BuildingTarget BuildingTarget { get; set; }
        public string BuildingModelName { get; set; }
        public Type BuildingType { get; set; }

        public static BuildingInfo GetBuildingInfo(IReadOnlyCollection<object> attributes)
        {
            var buildingInfo = new BuildingInfo();
            var isBuildingInfoPresent = false;

            // BinderModelName
            foreach (var buildingModelName in attributes.OfType<IBuildingModelNameProvider>())
            {
                isBuildingInfoPresent = true;
                if (buildingModelName?.Name != null)
                {
                    buildingInfo.BuildingModelName = buildingModelName.Name;
                    break;
                }
            }

            // BinderType
            foreach (var buildingTypeProviderMetadata in attributes.OfType<IBuildingTypeProviderMetadata>())
            {
                isBuildingInfoPresent = true;
                if (buildingTypeProviderMetadata.BuildingType != null)
                {
                    buildingInfo.BuildingType = buildingTypeProviderMetadata.BuildingType;
                    break;
                }
            }

            // BindingSource
            foreach (var buildingSourceMetadata in attributes.OfType<IBuildingTargetMetadata>())
            {
                isBuildingInfoPresent = true;
                if (buildingSourceMetadata.BuildingTarget != null)
                {
                    buildingInfo.BuildingTarget = buildingSourceMetadata.BuildingTarget;
                    break;
                }
            }

            return isBuildingInfoPresent ? buildingInfo : null;
        }
    }
}