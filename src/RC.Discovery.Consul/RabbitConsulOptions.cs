using Consul;
using System;

namespace Rabbit.Cloud.Discovery.Consul
{
    public class ConsulOptions
    {
        public string Address { get; set; }
        public string Datacenter { get; set; }
        public string Token { get; set; }
    }

    public class ConsulInstanceOptions
    {
        public string InstanceId { get; set; }
        public string ServiceName { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string HealthCheckInterval { get; set; }
        public string[] Tags { get; set; }
        public bool AutomaticRegistration { get; set; } = true;
    }

    public static class RabbitConsulOptionsExtensions
    {
        public static ConsulClient CreateClient(this ConsulOptions options)
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