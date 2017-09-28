using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Facade.Abstractions.MessageBuilding
{
    public class BuildingInfo
    {
        public string BuildingModelName { get; set; }
        public IBuilderTargetMetadata BuildingTarget { get; set; }

        public static BuildingInfo GetBuildingInfo(IReadOnlyCollection<object> attributes)
        {
            var buildingInfo = new BuildingInfo();
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

            // BuilderTarget
            foreach (var buildingSourceMetadata in attributes.OfType<IBuilderTargetMetadata>())
            {
                isBuildingInfoPresent = true;
                if (buildingSourceMetadata.BuildingTarget != null)
                {
                    buildingInfo.BuildingTarget = buildingSourceMetadata;
                    break;
                }
            }

            return isBuildingInfoPresent ? buildingInfo : null;
        }
    }
}