using Rabbit.Cloud.Facade.Abstractions.MessageBuilding;
using System;

namespace Rabbit.Cloud.Facade.Abstractions
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ToBodyAttribute : Attribute, IBuilderTargetMetadata
    {
/*        public ToBodyAttribute()
        {
        }

        public ToBodyAttribute(string formatter)
        {
            Formatter = formatter;
        }

        public string Formatter { get; set; }*/

        #region Implementation of IBuildingTargetMetadata

        public BuildingTarget BuildingTarget { get; } = BuildingTarget.Body;

        #endregion Implementation of IBuildingTargetMetadata
    }
}