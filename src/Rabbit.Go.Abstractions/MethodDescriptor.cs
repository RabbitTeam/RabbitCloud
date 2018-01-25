using Rabbit.Go.Interceptors;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rabbit.Go
{
    public class MethodDescriptor
    {
        public string Uri { get; set; }
        public Type ReturnType { get; set; }
        public IReadOnlyList<ParameterDescriptor> Parameters { get; set; }
        public string Method { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public Type ClienType { get; set; }
        public IList<InterceptorDescriptor> InterceptorDescriptors { get; set; }
    }

    public class ParameterDescriptor
    {
        public Type ParameterType { get; set; }
        public ParameterTarget Target { get; set; }
        public string Name { get; set; }
    }
}