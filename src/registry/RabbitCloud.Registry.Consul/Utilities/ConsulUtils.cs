using Consul;
using RabbitCloud.Abstractions;
using RabbitCloud.Registry.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RabbitCloud.Registry.Consul.Utilities
{
    public class ConsulUtils
    {
        public const string ServicePrefix = "rabbitrpc_";
        public const string ProtocolPrefix = "protocol_";

        public static string GetServiceId(ServiceRegistryDescriptor descriptor)
        {
            return $"{descriptor.Host}:{descriptor.Port}-{descriptor.ServiceKey.Name}";
        }

        public static bool IsRabbitService(AgentService agentService)
        {
            return agentService.Service.StartsWith(ServicePrefix) &&
                   agentService.Tags.Any(i => i.StartsWith(ProtocolPrefix));
        }

        public static AgentServiceRegistration GetServiceRegistration(ServiceRegistryDescriptor descriptor)
        {
            return new AgentServiceRegistration
            {
                Address = descriptor.Host,
                ID = GetServiceId(descriptor),
                Port = descriptor.Port,
                Name = GetConsulServiceName(descriptor),
                Tags = new[]
                {
                    ProtocolPrefix+descriptor.Protocol
                },
                Check = new AgentServiceCheck
                {
                    TTL = ConsulConstants.TtlInterval,
                    Status = HealthStatus.Passing
                }
            };
        }

        public static IEnumerable<ServiceRegistryDescriptor> GetServiceRegistryDescriptors(ServiceEntry[] serviceEntries)
        {
            return serviceEntries.Where(serviceEntry => serviceEntry.Checks.All(i => HealthStatus.Passing.Equals(i.Status)))
                .Select(i => GetServiceRegistryDescriptor(i.Service));
        }

        public static ServiceRegistryDescriptor GetServiceRegistryDescriptor(AgentService agentService)
        {
            var name = GetServiceNameByServiceId(agentService.ID);
            var group = GetGroupName(agentService.Service);

            return GetServiceRegistryDescriptor(group, name, agentService.Tags, agentService.Address,
                agentService.Port);
        }

        public static ServiceRegistryDescriptor GetServiceRegistryDescriptor(string group, string name, string[] tags, string host, int port, string version = "1.0.0")
        {
            return GetServiceRegistryDescriptor(group, name, GetProtocolByTags(tags), host, port, version);
        }

        public static ServiceRegistryDescriptor GetServiceRegistryDescriptor(string group, string name, string protocol, string host, int port, string version = "1.0.0")
        {
            return new ServiceRegistryDescriptor
            {
                Host = host,
                Port = port,
                Protocol = protocol,
                ServiceKey = new ServiceKey(group, name, version)
            };
        }

        public static string GetConsulServiceName(ServiceRegistryDescriptor descriptor)
        {
            return ServicePrefix + descriptor.ServiceKey.Group;
        }

        public static string GetGroupName(string consulServiceName)
        {
            return consulServiceName.Remove(0, ServicePrefix.Length);
        }

        public static string GetProtocolByTags(IEnumerable<string> tags)
        {
            return tags.Select(GetProtocolByTag).FirstOrDefault(i => i != null);
        }

        public static string GetProtocolByTag(string tag)
        {
            return tag.StartsWith(ProtocolPrefix) ? tag.Remove(0, ProtocolPrefix.Length) : null;
        }

        public static string GetServiceNameByServiceId(string consulServiceId)
        {
            var index = consulServiceId.LastIndexOf("-", StringComparison.Ordinal);
            return index == -1 ? null : consulServiceId.Substring(index + 1);
        }
    }
}