using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rabbit.Cloud.Client.Go
{
    public class ApplicationModel
    {
        public IList<GoClientModel> Clients { get; set; }
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();
    }

    public class GoClientModel
    {
        public GoClientModel(TypeInfo type, IReadOnlyList<object> attributes)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
        }

        public TypeInfo Type { get; }
        public string BaseUrl { get; set; }
        public IReadOnlyList<object> Attributes { get; }
        public IList<GoMethodModel> Methods { get; } = new List<GoMethodModel>();
    }

    public class GoMethodModel
    {
        public GoMethodModel(MethodInfo requestMethod, IReadOnlyList<object> attributes)
        {
            RequestMethod = requestMethod ?? throw new ArgumentNullException(nameof(requestMethod));
            Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
        }

        public GoClientModel RequestClient { get; set; }
        public MethodInfo RequestMethod { get; }
        public string Path { get; set; }
        public IList<GoParameterModel> Parameters { get; } = new List<GoParameterModel>();
        public IReadOnlyList<object> Attributes { get; }
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();
    }

    public class GoParameterModel
    {
        public GoParameterModel(ParameterInfo parameter, IReadOnlyList<object> attributes)
        {
            Parameter = parameter;
            Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
        }

        public GoMethodModel RequestMethod { get; set; }
        public string Name { get; set; }
        public ParameterInfo Parameter { get; }
        public IReadOnlyList<object> Attributes { get; set; }
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();
    }
}