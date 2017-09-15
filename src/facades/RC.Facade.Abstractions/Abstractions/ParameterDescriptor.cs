using Rabbit.Cloud.Facade.Abstractions.ModelBinding;
using System;

namespace Rabbit.Cloud.Facade.Abstractions
{
    public class ParameterDescriptor
    {
        public string Name { get; set; }
        public Type ParameterType { get; set; }
        public BindingInfo BindingInfo { get; set; }
    }
}