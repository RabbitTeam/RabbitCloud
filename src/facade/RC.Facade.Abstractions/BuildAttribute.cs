using Rabbit.Cloud.Facade.Abstractions.MessageBuilding;
using System;

namespace Rabbit.Cloud.Facade.Abstractions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BuildAttribute : Attribute, IBuilderModelNameProvider, IBuilderTypeProviderMetadata
    {
        public BuildAttribute()
        {
        }

        public BuildAttribute(Type builderType)
        {
            BuilderType = builderType;
        }

        public BuildAttribute(string name, Type builderType)
        {
            Name = name;
            BuilderType = builderType;
        }

        #region Implementation of IBuildingModelNameProvider

        public string Name { get; }

        #endregion Implementation of IBuildingModelNameProvider

        #region Implementation of IBuilderTypeProviderMetadata

        public Type BuilderType { get; }

        #endregion Implementation of IBuilderTypeProviderMetadata
    }
}