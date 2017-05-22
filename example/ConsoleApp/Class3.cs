using Microsoft.Extensions.DependencyInjection;
using RabbitCloud.Abstractions;
using RabbitCloud.Config.Abstractions;
using RabbitCloud.Registry.Abstractions;
using RabbitCloud.Rpc;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Formatter;
using RabbitCloud.Rpc.Abstractions.Proxy;
using RabbitCloud.Rpc.Cluster;
using RabbitCloud.Rpc.Cluster.HA;
using RabbitCloud.Rpc.Cluster.LoadBalance;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ConsoleApp
{
    internal class Class3
    {
        private readonly FormatterFactory _formatterFactory;
        private readonly ProtocolFactory _protocolFactory;
        private readonly RegistryTableFactory _registryTableFactory;
        private readonly IProxyFactory _proxyFactory;
        private readonly IServiceProvider _container;

        private ConcurrentDictionary<string, IProtocol> _protocols = new ConcurrentDictionary<string, IProtocol>();
        private ConcurrentDictionary<string, IRequestFormatter> _requestFormatters = new ConcurrentDictionary<string, IRequestFormatter>();
        private ConcurrentDictionary<string, IResponseFormatter> _responseFormatters = new ConcurrentDictionary<string, IResponseFormatter>();
        private ConcurrentDictionary<string, IRegistryTable> _registryTables = new ConcurrentDictionary<string, IRegistryTable>();

        public Class3(FormatterFactory formatterFactory, ProtocolFactory protocolFactory, RegistryTableFactory registryTableFactory, IProxyFactory proxyFactory, IServiceProvider container)
        {
            _formatterFactory = formatterFactory;
            _protocolFactory = protocolFactory;
            _registryTableFactory = registryTableFactory;
            _proxyFactory = proxyFactory;
            _container = container;
        }

        public IRequestFormatter GetRequestFormatter(string name)
        {
            return _requestFormatters.GetOrAdd(name, _formatterFactory.GetRequestFormatter(name));
        }

        public IResponseFormatter GetResponseFormatter(string name)
        {
            return _responseFormatters.GetOrAdd(name, _formatterFactory.GetResponseFormatter(name));
        }

        public IProtocol GetProtocol(ProtocolConfig config)
        {
            return _protocols.GetOrAdd(config.Name, _protocolFactory.GetProtocol(config.Name));
        }

        public IRegistryTable GetRegistryTable(RegistryConfig config)
        {
            return _registryTables.GetOrAdd(config.Name, k => _registryTableFactory.GetRegistryTable(config.Protocol, new Dictionary<string, string>
            {
                {"Address",config.Address }
            }));
        }

        public async Task<IExporter> Export(ServiceConfig config)
        {
            var uri = new Uri(config.Export);
            var protocolName = uri.Scheme;
            var port = uri.Port;
            var host = uri.Host;

            var registryTable = GetRegistryTable(new RegistryConfig
            {
                Name = config.Registry
            });
            var protocol = GetProtocol(new ProtocolConfig
            {
                Name = protocolName
            });
            var serviceType = Type.GetType(config.Interface);
            var export = protocol.Export(new ExportContext
            {
                Caller = new TypeCaller(serviceType, () => _container.GetRequiredService(serviceType)),
                EndPoint = new IPEndPoint(IPAddress.Parse(host), port),
                ServiceKey = new ServiceKey(config.Group, config.Id, "1.0.0")
            });

            await registryTable.RegisterAsync(new ServiceRegistryDescriptor
            {
                Host = host,
                Port = port,
                Protocol = protocolName,
                ServiceKey = new ServiceKey(config.Group, config.Id, "1.0.0")
            });

            return export;
        }

        public async Task<T> Referer<T>(RefererConfig config)
        {
            return _proxyFactory.GetProxy<T>(await Referer(config));
        }

        public async Task<ICaller> Referer(RefererConfig config)
        {
            var serviceType = Type.GetType(config.Interface);
            var registryTable = GetRegistryTable(new RegistryConfig { Name = config.Registry });
            var protocolName = config.Protocol;
            var protocol = GetProtocol(new ProtocolConfig
            {
                Name = protocolName
            });

            var descriptors = await registryTable.Discover(new ServiceRegistryDescriptor
            {
                ServiceKey = new ServiceKey(config.Group, config.Id, "1.0.0")
            });

            var refers = new List<ICaller>();
            foreach (var descriptor in descriptors)
            {
                var refer = protocol.Refer(new ReferContext
                {
                    EndPoint = new IPEndPoint(IPAddress.Parse(descriptor.Host), descriptor.Port),
                    ServiceKey = descriptor.ServiceKey
                });
                refers.Add(refer);
            }

            return new DefaultCluster
            {
                HaStrategy = new FailfastHaStrategy(refers),
                LoadBalance = new RoundRobinLoadBalance()
            };
        }
    }
}