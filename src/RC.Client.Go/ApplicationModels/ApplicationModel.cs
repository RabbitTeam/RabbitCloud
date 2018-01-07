using Rabbit.Cloud.Client.Go.Abstractions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rabbit.Cloud.Client.Go.ApplicationModels
{
    public class ApplicationModel
    {
        public IList<ServiceModel> Services { get; } = new List<ServiceModel>();
    }

    public class ServiceModel
    {
        public ServiceModel(TypeInfo serviceType, IReadOnlyList<object> attributes)
        {
            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));

            Type = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            Attributes = new List<object>(attributes);
            Properties = new Dictionary<object, object>();
        }

        public TypeInfo Type { get; }
        public string Url { get; set; }
        public IReadOnlyList<object> Attributes { get; }
        public IDictionary<object, object> Properties { get; }
        public IList<RequestModel> Requests { get; } = new List<RequestModel>();
    }

    public struct RequestPath
    {
        public RequestPath(string pathTemplate)
        {
            PathTemplate = pathTemplate;
            Variables = TemplateEngine.GetVariables(pathTemplate);
        }

        public string PathTemplate { get; }
        public IReadOnlyList<string> Variables { get; }

        #region Overrides of ValueType

        /// <summary>Returns the fully qualified type name of this instance.</summary>
        /// <returns>The fully qualified type name.</returns>
        public override string ToString()
        {
            return PathTemplate;
        }

        #endregion Overrides of ValueType
    }

    public class RequestModel
    {
        public RequestModel(MethodInfo methodInfo, IReadOnlyList<object> attributes)
        {
            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));

            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            Attributes = new List<object>(attributes);
            Properties = new Dictionary<object, object>();
            Parameters = new List<ParameterModel>();
        }

        public IReadOnlyList<object> Attributes { get; }
        public IDictionary<object, object> Properties { get; }
        public MethodInfo MethodInfo { get; }
        public IList<ParameterModel> Parameters { get; set; }
        public ServiceModel ServiceModel { get; set; }
        public RequestPath Path { get; set; }

        public Type RequesType { get; set; }
        public Type ResponseType { get; set; }
    }

    public class ParameterModel
    {
        public ParameterModel(ParameterInfo parameterInfo, IReadOnlyList<object> attributes)
        {
            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));

            ParameterInfo = parameterInfo ?? throw new ArgumentNullException(nameof(parameterInfo));
            Attributes = new List<object>(attributes);
            Properties = new Dictionary<object, object>();
        }

        public RequestModel Request { get; set; }
        public IReadOnlyList<object> Attributes { get; }
        public IDictionary<object, object> Properties { get; }
        public ParameterInfo ParameterInfo { get; }
        public string ParameterName { get; set; }
        public ParameterTarget Target { get; set; }
    }
}