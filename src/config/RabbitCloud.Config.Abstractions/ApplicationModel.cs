using RabbitCloud.Registry.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Proxy;
using System;
using System.Linq;

namespace RabbitCloud.Config.Abstractions
{
    public class CallerEntry
    {
        public RefererConfig RefererConfig { get; set; }
        public IProtocol Protocol { get; set; }
        public IRegistryTable RegistryTable { get; set; }
        public ICaller Caller { get; set; }
    }

    public class ServiceEntry
    {
        public ServiceConfig ServiceConfig { get; set; }
        public IProtocol Protocol { get; set; }
        public IRegistryTable RegistryTable { get; set; }
        public IExporter Exporter { get; set; }
    }

    public class ProtocolEntry
    {
        public ProtocolConfig ProtocolConfig { get; set; }
        public IProtocol Protocol { get; set; }
    }

    public class RegistryTableEntry
    {
        public RegistryConfig RegistryConfig { get; set; }
        public IRegistryTable RegistryTable { get; set; }
    }

    public class ApplicationModel
    {
        private readonly IProxyFactory _proxyFactory;

        public ApplicationModel(IProxyFactory proxyFactory)
        {
            _proxyFactory = proxyFactory;
        }

        public ProtocolEntry[] Protocols { get; set; }
        public RegistryTableEntry[] RegistryTables { get; set; }
        public CallerEntry[] CallerEntries { get; set; }
        public ServiceEntry[] ServiceEntries { get; set; }

        public CallerEntry GetCallerEntry(string id)
        {
            return CallerEntries.SingleOrDefault(i => string.Equals(i.RefererConfig.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public ServiceEntry GetServiceEntry(string id)
        {
            return ServiceEntries.SingleOrDefault(i => string.Equals(i.ServiceConfig.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public ProtocolEntry GetProtocol(string id)
        {
            return Protocols.SingleOrDefault(i => string.Equals(i.ProtocolConfig.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public RegistryTableEntry GetRegistryTable(string name)
        {
            return RegistryTables.SingleOrDefault(i => string.Equals(i.RegistryConfig.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public T Referer<T>(string id)
        {
            return _proxyFactory.GetProxy<T>(GetCallerEntry(id).Caller);
        }
    }

    public class ApplicationModelDescriptor
    {
        public ProtocolConfig[] Protocols { get; set; }
        public ServiceConfig[] Services { get; set; }
        public RefererConfig[] Referers { get; set; }
        public RegistryConfig[] Registrys { get; set; }
    }
}