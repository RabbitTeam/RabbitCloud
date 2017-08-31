using Consul;
using RabbitCloud.Registry.Abstractions;
using System;
using System.Collections.Generic;

namespace RabbitCloud.Registry.Consul
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

        public static ConsulRegistration Create(string serviceName, Uri uri, IDictionary<string, string> metadata = null)
        {
            var agentServiceRegistration = new AgentServiceRegistration
            {
                Address = uri.Host.ToLower(),
                Port = uri.Port,
                Name = serviceName,
                ID = GetInstanceId(uri)
            };

            return new ConsulRegistration(agentServiceRegistration);
        }

        #endregion Public Static Method

        #region Private Method

        private static string GetInstanceId(Uri uri)
        {
            return GetUrl(uri.Scheme, uri.Host, uri.Port);
        }

        private static string GetUrl(string scheme, string host, int port)
        {
            return $"{scheme}://{host}:{port}".ToLower();
        }

        #endregion Private Method
    }
}