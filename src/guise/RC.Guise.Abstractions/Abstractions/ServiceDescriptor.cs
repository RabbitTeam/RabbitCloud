using Rabbit.Cloud.Guise.Abstractions.Filters;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Guise.Abstractions
{
    public class ServiceDescriptor
    {
        public ServiceDescriptor(string id)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Properties = new Dictionary<object, object>();
        }

        public string Id { get; }
        public string HttpMethod { get; set; }

        public ServiceRouteInfo ServiceRouteInfo { get; set; }

        public IList<ParameterDescriptor> Parameters { get; set; }
        public IList<FilterDescriptor> FilterDescriptors { get; set; }
        public virtual string DisplayName { get; set; }
        public IDictionary<object, object> Properties { get; set; }
    }
}