using Rabbit.Cloud.Facade.Abstractions.MessageBuilding;
using System;

namespace Rabbit.Cloud.Facade.Abstractions
{
    public class ParameterDescriptor
    {
        public string Name { get; set; }
        public Type ParameterType { get; set; }
        public BuildingInfo BuildingInfo { get; set; }
    }
}