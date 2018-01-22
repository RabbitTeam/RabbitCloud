using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rabbit.Go.Abstractions
{
    public class MethodDescriptor
    {
        public IDictionary<string, StringValues> DefaultQuery { get; } = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
        public IDictionary<string, StringValues> DefaultHeaders { get; } = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
        public string Uri { get; set; }
        public Type ReturnType { get; set; }
        public IReadOnlyList<ParameterDescriptor> Parameters { get; set; }
        public string Method { get; set; }

        public MethodInfo MethodInfo { get; set; }
        public Type ClienType { get; set; }
    }

    public class ParameterDescriptor
    {
        public Type ParameterType { get; set; }
        public ParameterTarget Target { get; set; }
        public string Name { get; set; }
    }
}