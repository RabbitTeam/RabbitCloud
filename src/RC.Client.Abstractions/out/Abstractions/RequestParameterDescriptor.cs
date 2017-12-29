using System;

namespace Rabbit.Cloud.Client.Abstractions
{
    public class RequestParameterDescriptor
    {
        public string Name { get; set; }
        public Type ParameterType { get; set; }
    }
}