using Rabbit.Cloud.Facade.Abstractions.MessageBuilding;
using System;

namespace Rabbit.Cloud.Facade.Abstractions
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true)]
    public class ToQueryAttribute : Attribute, IBuilderTargetMetadata, IBuilderModelNameProvider
    {
        public ToQueryAttribute()
        {
        }

        public ToQueryAttribute(string name)
        {
            Name = name;
        }

        public ToQueryAttribute(string name, string value)
        {
            Name = name;
            Value = value;
            BuildingTarget = BuildingTarget.Custom;
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