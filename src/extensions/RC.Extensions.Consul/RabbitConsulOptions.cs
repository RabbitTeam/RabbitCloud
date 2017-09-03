using Consul;
using System;
using System.Linq;

namespace Rabbit.Cloud.Extensions.Consul
{
    public class RabbitConsulOptions
    {
        public string Address { get; set; }
        public string Datacenter { get; set; }
        public string Token { get; set; }
        public DiscoveryOptions Discovery { get; set; }

        public class DiscoveryOptions
        {
            public class HealthCheck
            {
                public string Url { get; set; }
                public string Interval { get; set; }
            }

            public string InstanceId { get; set; }
            public string ServiceName { get; set; }
            public string HostName { get; set; }
            public int Port { get; set; }
            public bool IsSecure { get; set; }
            private HealthCheck[] _healthChecks;

            public HealthCheck[] HealthChecks
            {
                get
                {
                    if (string.IsNullOrEmpty(HealthCheckUrl))
                        return _healthChecks;

                    var healthChecks = _healthChecks ?? Enumerable.Empty<HealthCheck>();
                    if (string.IsNullOrEmpty(HealthCheckInterval))
                        HealthCheckInterval = "10s";

                    return new[]
                    {
                        new HealthCheck
                        {
                            Url = HealthCheckUrl,
                            Interval = HealthCheckInterval
                        }
                    }.Concat(healthChecks).ToArray();
                }
                set => _healthChecks = value;
            }

            public string HealthCheckUrl { get; set; }
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