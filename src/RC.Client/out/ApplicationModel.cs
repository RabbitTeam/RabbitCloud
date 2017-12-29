using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rabbit.Cloud.Client
{
    public class ApplicationModel
    {
        public IList<RequestClientModel> Clients { get; set; }
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();
    }

    public class RequestClientModel
    {
        public RequestClientModel(TypeInfo type, IReadOnlyList<object> attributes)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
        }

        public TypeInfo Type { get; }
        public string BaseUrl { get; set; }
        public IReadOnlyList<object> Attributes { get; }
        public IList<RequestMethodModel> Methods { get; } = new List<RequestMethodModel>();
    }

    public class RequestMethodModel
    {
        public RequestMethodModel(MethodInfo requestMethod, IReadOnlyList<object> attributes)
        {
            RequestMethod = requestMethod ?? throw new ArgumentNullException(nameof(requestMethod));
            Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
        }

        public RequestClientModel RequestClient { get; set; }
        public MethodInfo RequestMethod { get; }
        public string Path { get; set; }
        public IList<RequestParameterModel> Parameters { get; } = new List<RequestParameterModel>();
        public IReadOnlyList<object> Attributes { get; }
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();
    }

    public class RequestParameterModel
    {
        public RequestParameterModel(ParameterInfo parameter, IReadOnlyList<object> attributes)
        {
            Parameter = parameter;
            Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
        }

        public RequestMethodModel RequestMethod { get; set; }
        public string Name { get; set; }
        public ParameterInfo Parameter { get; }
        public IReadOnlyList<object> Attributes { get; set; }
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();
    }
}