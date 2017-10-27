using Consul;
using Rabbit.Cloud.Discovery.Abstractions;

namespace Rabbit.Cloud.Discovery.Consul.Registry
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