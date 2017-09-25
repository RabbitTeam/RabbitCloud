using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Facade.Abstractions.MessageBuilding
{
    public class BuildingInfo
    {
        public BuildingTarget BuildingTarget { get; set; }
        public string BuildingModelName { get; set; }
        public Type BuilderType { get; set; }
        public IEnumerable<IBuilderTargetMetadata> Metadatas { get; set; }

        public static BuildingInfo GetBuildingInfo(IReadOnlyCollection<object> attributes)
        {
            var buildingInfo = new BuildingInfo
            {
                Metadatas = attributes.OfType<IBuilderTargetMetadata>().ToArray()
            };
            var isBuildingInfoPresent = false;

            // BinderModelName
            foreach (var buildingModelName in attributes.OfType<IBuilderModelNameProvider>())
            {
                isBuildingInfoPresent = true;
                if (buildingModelName?.Name != null)
                {
                    buildingInfo.BuildingModelName = buildingModelName.Name;
                    break;
                }
            }

            // BinderType
            foreach (var builderTypeProviderMetadata in attributes.OfType<IBuilderTypeProviderMetadata>())
            {
                isBuildingInfoPresent = true;
                if (builderTypeProviderMetadata.BuilderType != null)
                {
                    buildingInfo.BuilderType = builderTypeProviderMetadata.BuilderType;
                    break;
                }
            }

            // BindingSource
            foreach (var buildingSourceMetadata in attributes.OfType<IBuilderTargetMetadata>())
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