using Consul;
using Rabbit.Cloud.Extensions.Consul.Discovery;
using Rabbit.Cloud.Extensions.Consul.Registry;
using RC.Abstractions.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Extensions.Consul.Utilities
{
    public class ConsulUtil
    {
        public const string ServicePrefix = "rabbitcloud";
        public static readonly TimeSpan TtlInterval = TimeSpan.FromSeconds(30);

        #region Public  Method

        public static ConsulRegistration Create(RabbitConsulOptions.DiscoveryOptions options, IDictionary<string, string> metadata = null)
        {
            var tags = options.Tags ?? Enumerable.Empty<string>();

            if (options.IsSecure)
                tags = tags.Concat(new[] { "https" });

            return new ConsulRegistration(new AgentServiceRegistration
            {
                Address = options.HostName.ToLower(),
                Port = options.Port,
                Name = options.ServiceName,
                ID = GetInstanceId(options.InstanceId),
                Tags = tags.ToArray(),
                Check = new AgentServiceCheck
                {
                    TTL = TimeUtil.GetTimeSpanBySimple(options.HealthCheckInterval),
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
                IsSecure = agentService.Tags?.Contains("https") ?? false,
                Metadata = new Dictionary<string, string>()
            };
            instance.Uri = new Uri(GetUrl(instance.IsSecure, instance.Host, instance.Port));

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

        private static string GetUrl(bool isSecure, string host, int port)
        {
            return GetUrl(isSecure ? "https" : "http", host, port);
        }

        private static string GetUrl(string scheme, string host, int port)
        {
            return $"{scheme}://{host}:{port}".ToLower();
        }

        #endregion Private Method
    }
}