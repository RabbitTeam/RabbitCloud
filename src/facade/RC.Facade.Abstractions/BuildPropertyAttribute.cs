using Rabbit.Cloud.Facade.Abstractions.MessageBuilding;
using System;

namespace Rabbit.Cloud.Facade.Abstractions
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class BuildPropertyAttribute : Attribute, IBuilderTypeProviderMetadata, IBuilderModelNameProvider
    {
        public BuildPropertyAttribute(Type builderType)
        {
            BuilderType = builderType;
        }

        public BuildPropertyAttribute(Type builderType, string name)
        {
            BuilderType = builderType;
            Name = name;
        }

        #region Implementation of IBuilderTypeProviderMetadata

        public Type BuilderType { get; }

        #endregion Implementation of IBuilderTypeProviderMetadata

        #region Implementation of IBuildingModelNameProvider

        public string Name { get; }

        #endregion Implementation of IBuildingModelNameProvider
    }
}