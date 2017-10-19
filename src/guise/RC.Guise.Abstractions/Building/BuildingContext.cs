using Rabbit.Cloud.Client.Abstractions;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Guise.Abstractions.Building
{
    public class BuildingContext
    {
        public BuildingContext(IRabbitContext rabbitContext, ServiceDescriptor serviceDescriptor)
        {
            RabbitContext = rabbitContext ?? throw new ArgumentNullException(nameof(rabbitContext));
            ServiceDescriptor = serviceDescriptor ?? throw new ArgumentNullException(nameof(serviceDescriptor));
        }

        public IRabbitContext RabbitContext { get; }
        public ServiceDescriptor ServiceDescriptor { get; }

        public IDictionary<string, object> Arguments { get; set; }
    }
}