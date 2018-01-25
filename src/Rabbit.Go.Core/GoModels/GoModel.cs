using Rabbit.Go.Interceptors;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rabbit.Go.Core.GoModels
{
    public class GoModel
    {
        public GoModel()
        {
            Types = new List<TypeModel>();
            Properties = new Dictionary<object, object>();
            Interceptors = new List<IInterceptorMetadata>();
        }

        public IList<TypeModel> Types { get; }
        public IList<IInterceptorMetadata> Interceptors { get; }
        public IDictionary<object, object> Properties { get; }
    }

    public class TypeModel
    {
        public TypeModel(Type type, IReadOnlyList<object> attributes)
        {
            Type = type;
            Attributes = attributes;
            Properties = new Dictionary<object, object>();
            Attributes = new List<object>(attributes);
            Interceptors = new List<IInterceptorMetadata>();
            Methods = new List<MethodModel>();
        }

        public GoModel Go { get; set; }
        public Type Type { get; }
        public IList<MethodModel> Methods { get; }
        public IReadOnlyList<object> Attributes { get; }
        public IDictionary<object, object> Properties { get; }
        public IList<IInterceptorMetadata> Interceptors { get; }
    }

    public class MethodModel
    {
        public MethodModel(MethodInfo method, IReadOnlyList<object> attributes)
        {
            Method = method;
            Attributes = attributes;
            Properties = new Dictionary<object, object>();
            Attributes = new List<object>(attributes);
            Interceptors = new List<IInterceptorMetadata>();
            Parameters = new List<ParameterModel>();
        }

        public MethodInfo Method { get; set; }
        public TypeModel Type { get; set; }
        public IReadOnlyList<object> Attributes { get; }
        public IDictionary<object, object> Properties { get; }
        public IList<IInterceptorMetadata> Interceptors { get; }
        public IList<ParameterModel> Parameters { get; }
    }

    public class ParameterModel
    {
        public ParameterModel(ParameterInfo parameterInfo, IReadOnlyList<object> attributes)
        {
            ParameterInfo = parameterInfo;
            Attributes = attributes;
            Properties = new Dictionary<object, object>();
            Attributes = new List<object>(attributes);
        }

        public MethodModel Method { get; set; }
        public IReadOnlyList<object> Attributes { get; }
        public IDictionary<object, object> Properties { get; }
        public ParameterInfo ParameterInfo { get; }
        public string ParameterName { get; set; }
        public ParameterTarget Target { get; set; }
    }
}