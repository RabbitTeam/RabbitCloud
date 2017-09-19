using Rabbit.Cloud.Facade.Abstractions.MessageBuilding;
using System;

namespace Rabbit.Cloud.Facade.Abstractions
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true)]
    public class ToQueryAttribute : Attribute, IBuildingTargetMetadata, IBuildingModelNameProvider
    {
        public ToQueryAttribute()
        {
        }

        public ToQueryAttribute(string name, string value = null)
        {
            Name = name;
            Value = value;
        }

        #region Implementation of IBuildingTargetMetadata

        public BuildingTarget BuildingTarget { get; } = BuildingTarget.Query;

        #endregion Implementation of IBuildingTargetMetadata

        #region Implementation of IBuildingModelNameProvider

        public string Name { get; }

        #endregion Implementation of IBuildingModelNameProvider

        public object Value { get; set; }
    }
}