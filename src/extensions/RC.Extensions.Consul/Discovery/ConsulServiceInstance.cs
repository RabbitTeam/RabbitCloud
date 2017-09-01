using Consul;
using Rabbit.Cloud.Discovery.Abstractions;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Extensions.Consul.Discovery
{
    internal class ConsulServiceInstance : IServiceInstance
    {
        #region Implementation of IServiceInstance

        public string ServiceId { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool IsSecure { get; set; }
        public Uri Uri { get; set; }
        public IDictionary<string, string> Metadata { get; set; }

        #endregion Implementation of IServiceInstance

        #region Public Static Method

        public static ConsulServiceInstance Create(AgentService agentService)
        {
            Uri.TryCreate(agentService.ID, UriKind.Absolute, out var uri);
            var instance = new ConsulServiceInstance
            {
                ServiceId = agentService.Service,
                Host = agentService.Address.ToLower(),
                Port = agentService.Port,
                IsSecure = uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase),
                Metadata = new Dictionary<string, string>()
            };
            instance.Uri = new Uri(GetUrl(instance.IsSecure, instance.Host, instance.Port));

            return instance;
        }

        #endregion Public Static Method

        #region Private Method

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