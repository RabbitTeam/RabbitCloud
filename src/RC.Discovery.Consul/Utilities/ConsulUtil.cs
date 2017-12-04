using Consul;
using Rabbit.Cloud.Abstractions.Utilities;
using Rabbit.Cloud.Discovery.Consul.Discovery;
using Rabbit.Cloud.Discovery.Consul.Registry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Discovery.Consul.Utilities
{
    public class ConsulUtil
    {
        public const string ServicePrefix = "rabbitcloud";
        public static readonly TimeSpan TtlInterval = TimeSpan.FromSeconds(30);
        private static readonly Dictionary<string, string> EmptyMetadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        #region Public  Method

        public static ConsulRegistration Create(ConsulDiscoveryOptions options, IDictionary<string, string> metadata = null)
        {
            var tags = options.Tags ?? Enumerable.Empty<string>();

            tags = tags.Concat(new[] { ServicePrefix });

            return new ConsulRegistration(new AgentServiceRegistration
            {
                Address = options.HostName.ToLower(),
                Port = options.Port,
                Name = options.ServiceName,
                ID = GetInstanceId(options.InstanceId),
                Tags = tags.Distinct().ToArray(),
                Check = new AgentServiceCheck
                {
                    TTL = TimeUtilities.GetTimeSpanBySimple(options.HealthCheckInterval),
                    Status = HealthStatus.Passing
                }
            });
        }

        public static ConsulServiceInstance Create(AgentService agentService)
        {
            if (!IsRabbitCloudService(agentService))
                return null;
            var instance = new ConsulServiceInstance
            {
                ServiceId = agentService.Service,
                Host = agentService.Address.ToLower(),
                Port = agentService.Port,
                Metadata = EmptyMetadata
            };

            return instance;
        }

        #endregion Public  Method

        #region Private Method

        private static bool IsRabbitCloudService(AgentService agentService)
        {
            return agentService.ID.StartsWith(ServicePrefix);
        }

        private static string GetInstanceId(string instanceId)
        {
            return $"{ServicePrefix}:{instanceId}";
        }

        #endregion Private Method
    }
}