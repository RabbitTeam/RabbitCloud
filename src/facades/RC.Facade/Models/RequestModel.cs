using Rabbit.Cloud.Facade.Abstractions.Filters;
using System.Collections.Generic;
using System.Reflection;

namespace Rabbit.Cloud.Facade.Models
{
    public class RequestModel
    {
        public RequestModel(MethodInfo method, IReadOnlyList<object> attributes)
        {
            Method = method;
            Attributes = attributes;

            Filters = new List<IFilterMetadata>();
            Parameters = new List<ParameterModel>();
        }

        public string RouteUrl { get; set; }
        public MethodInfo Method { get; }
        public IReadOnlyList<object> Attributes { get; }
        public ServiceModel Service { get; set; }
        public IList<IFilterMetadata> Filters { get; }
        public IList<ParameterModel> Parameters { get; }
    }
}