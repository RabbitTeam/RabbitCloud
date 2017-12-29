using Rabbit.Cloud.Client.Abstractions.ServiceInstanceChooser;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Abstractions
{
    public class RabbitClientOptions
    {
        public IServiceInstanceChooserCollection Choosers { get; } = new ServiceInstanceChooserCollection();

        public IServiceInstanceChooser DefaultChooser { get; } = new RandomServiceInstanceChooser();

        public IDictionary<string, ServiceRequestOptions> RequestOptionses { get; } = new Dictionary<string, ServiceRequestOptions>();

        public ServiceRequestOptions DefaultRequestOptions { get; } = new ServiceRequestOptions();
    }

    public class ServiceRequestOptions
    {
        public ServiceRequestOptions()
        {
            MaxAutoRetries = 3;
            MaxAutoRetriesNextServer = 1;
            ReadTimeout = TimeSpan.FromSeconds(10);
            ConnectionTimeout = TimeSpan.FromSeconds(2);
            ServiceChooser = "random";
        }

        public int MaxAutoRetries { get; set; }
        public int MaxAutoRetriesNextServer { get; set; }
        public string ServiceChooser { get; set; }
        public TimeSpan ConnectionTimeout { get; set; }
        public TimeSpan ReadTimeout { get; set; }
    }
}