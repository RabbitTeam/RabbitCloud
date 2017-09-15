using Rabbit.Cloud.Facade.Abstractions.Filters;
using System.Collections.Generic;
using System.Reflection;

namespace Rabbit.Cloud.Facade.Models
{
    public class ServiceModel
    {
        public ServiceModel(TypeInfo serviceType, IReadOnlyList<object> attributes)
        {
            ServiceType = serviceType;
            Attributes = attributes;

            Requests = new List<RequestModel>();
            Filters = new List<IFilterMetadata>();
        }

        public IList<RequestModel> Requests { get; }
        public ApplicationModel Application { get; set; }
        public IReadOnlyList<object> Attributes { get; }
        public string ServiceName { get; set; }
        public TypeInfo ServiceType { get; }
        public IList<IFilterMetadata> Filters { get; }
    }
}