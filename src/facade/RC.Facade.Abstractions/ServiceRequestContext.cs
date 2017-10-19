using Microsoft.AspNetCore.Routing;
using Rabbit.Cloud.Client.Abstractions;
using System.Collections.Generic;

namespace Rabbit.Cloud.Facade.Abstractions
{
    public class ServiceRequestContext
    {
        public ServiceRequestContext()
        {
        }

        public ServiceRequestContext(IRabbitContext rabbitContext, IDictionary<string, object> arguments, RouteData routeData, ServiceDescriptor serviceDescriptor)
        {
            RabbitContext = rabbitContext;
            Arguments = arguments;
            RouteData = routeData;
            ServiceDescriptor = serviceDescriptor;
        }

        public IDictionary<string, object> Arguments { get; set; }
        public ServiceDescriptor ServiceDescriptor { get; set; }
        public IRabbitContext RabbitContext { get; set; }
        public RouteData RouteData { get; set; }
    }
}