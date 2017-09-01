using Consul;
using Rabbit.Cloud.Registry.Abstractions;
using System.Collections.Generic;

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
            return new ConsulRegistration(new AgentServiceRegistration
            {
                Address = options.HostName.ToLower(),
                Port = options.Port,
                Name = options.ServiceName,
                ID = options.InstanceId,
                Tags = options.IsSecure ? new[] { "https" } : null
            });
        }

        #endregion Public Static Method
    }
}