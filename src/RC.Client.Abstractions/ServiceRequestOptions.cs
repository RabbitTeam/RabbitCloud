using System;

namespace Rabbit.Cloud.Client.Abstractions
{
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