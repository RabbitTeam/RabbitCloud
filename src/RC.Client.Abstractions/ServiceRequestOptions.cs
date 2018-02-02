using System;

namespace Rabbit.Cloud.Client.Abstractions
{
    public class ServiceRequestOptions
    {
        public ServiceRequestOptions()
        {
            MaxAutoRetries = 3;
            MaxAutoRetriesNextServer = 1;
            Timeout = TimeSpan.FromSeconds(10);
            ServiceChooser = "random";
            SerializerName = "json";
        }

        public int MaxAutoRetries { get; set; }
        public int MaxAutoRetriesNextServer { get; set; }
        public string ServiceChooser { get; set; }
        public TimeSpan Timeout { get; set; }
        public string SerializerName { get; set; }
    }
}