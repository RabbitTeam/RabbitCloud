using Rabbit.Cloud.Facade.Abstractions.MessageBuilding;
using System;

namespace Rabbit.Cloud.Facade.Abstractions
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true)]
    public class ToHeaderAttribute : Attribute, IBuilderTargetMetadata, IBuilderModelNameProvider
    {
        public ToHeaderAttribute()
        {
        }

        public ToHeaderAttribute(string name, string value = null)
        {
            Name = name;
            Value = value;
        }

        #region Implementation of IBuildingTargetMetadata

        public BuildingTarget BuildingTarget { get; } = BuildingTarget.Header;

        #endregion Implementation of IBuildingTargetMetadata

        #region Implementation of IModelNameProvider

        public string Name { get; }

        #endregion Implementation of IModelNameProvider

        public object Value { get; set; }
    }
}