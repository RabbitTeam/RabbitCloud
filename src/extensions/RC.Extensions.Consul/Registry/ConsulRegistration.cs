using Consul;
using Rabbit.Cloud.Registry.Abstractions;

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
    }
}