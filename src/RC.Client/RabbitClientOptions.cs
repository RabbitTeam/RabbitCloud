using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Internal.Serialization;
using Rabbit.Cloud.Client.Serialization;
using Rabbit.Cloud.Client.ServiceInstanceChooser;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client
{
    public class RabbitClientOptions
    {
        public RabbitClientOptions()
        {
            SerializerTable.Set("json", new JsonSerializer());
        }

        public IServiceInstanceChooserCollection Choosers { get; } = new ServiceInstanceChooserCollection();

        public IServiceInstanceChooser DefaultChooser { get; } = new RandomServiceInstanceChooser();

        public IDictionary<string, ServiceRequestOptions> RequestOptionses { get; } = new Dictionary<string, ServiceRequestOptions>();

        public ServiceRequestOptions DefaultRequestOptions { get; } = new ServiceRequestOptions();

        public SerializerTable SerializerTable { get; } = new SerializerTable();
    }
}