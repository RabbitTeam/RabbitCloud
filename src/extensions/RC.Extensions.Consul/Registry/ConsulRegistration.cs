using Consul;
using Rabbit.Cloud.Registry.Abstractions;
using RC.Abstractions.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Extensions.Consul.Registry
{
    public class ConsulRegistration : IRegistration
    {
        public ConsulRegistration(AgentServiceRegistration agentServiceRegistration)
        {
            AgentServiceRegistration = agentServiceRegistration;
        }

        #region Implementation of IRegistration

        public string ServiceId => AgentServiceRegistration.Name;

        #endregion Implementation of IRegistration

        public AgentServiceRegistration AgentServiceRegistration { get; }

        public string InstanceId => AgentServiceRegistration.ID;

        #region Public Static Method

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
                ID = options.InstanceId,
                Tags = tags.ToArray(),
                Check = new AgentServiceCheck
                {
                    HTTP = options.HealthCheckUrl,
                    Interval = TimeUtil.GetTimeSpanBySimple(options.HealthCheckInterval)
                }
            });
        }

        #endregion Public Static Method
    }
}