using Consul;
using System;

namespace Rabbit.Cloud.Discovery.Consul
{
    public class RabbitConsulOptions
    {
        public string Address { get; set; }
        public string Datacenter { get; set; }
        public string Token { get; set; }
        public DiscoveryOptions Discovery { get; set; }

        public class DiscoveryOptions
        {
            public string InstanceId { get; set; }
            public string ServiceName { get; set; }
            public string HostName { get; set; }
            public int Port { get; set; }
            public bool IsSecure { get; set; }
            public string HealthCheckInterval { get; set; }
            public string[] Tags { get; set; }
        }
    }

    public static class RabbitConsulOptionsExtensions
    {
        public static ConsulClient CreateClient(this RabbitConsulOptions options)
        {
            return new ConsulClient(s =>
            {
                s.Address = new Uri(options.Address);
                s.Datacenter = options.Datacenter;
                s.Token = options.Token;
            });
        }
    }
}