using Microsoft.Extensions.DependencyInjection;
using RabbitCloud.Abstractions;
using RabbitCloud.Config.Abstractions;
using RabbitCloud.Config.Abstractions.Adapter;
using RabbitCloud.Registry.Abstractions;
using RabbitCloud.Rpc;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Proxy;
using RabbitCloud.Rpc.Cluster;
using RabbitCloud.Rpc.Cluster.HA;
using RabbitCloud.Rpc.Cluster.LoadBalance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RabbitCloud.Config
{
    public class DefaultApplicationFactory : IApplicationFactory
    {
        private readonly IServiceProvider _container;
        private readonly IRegistryTableFactory _registryTableFactory;
        private readonly IProtocolFactory _protocolFactory;
        private readonly IProxyFactory _proxyFactory;

        public DefaultApplicationFactory(IServiceProvider container)
        {
            _container = container;
            _registryTableFactory = container.GetRequiredService<IRegistryTableFactory>();
            _protocolFactory = container.GetRequiredService<IProtocolFactory>();
            _proxyFactory = container.GetRequiredService<IProxyFactory>();
        }

        #region Implementation of IApplicationFactory

        public async Task<ApplicationModel> CreateApplicationAsync(ApplicationModelDescriptor descriptor)
        {
            var applicationModel = new ApplicationModel(_proxyFactory)
            {
                RegistryTables = descriptor.Registrys.ToDictionary(i => i, _registryTableFactory.GetRegistryTable).Select(
                    i => new RegistryTableEntry
                    {
                        RegistryConfig = i.Key,
                        RegistryTable = i.Value
                    }).ToArray(),
                Protocols = descriptor.Protocols.ToDictionary(i => i, _protocolFactory.GetProtocol).Select(
                    i => new ProtocolEntry
                    {
                        ProtocolConfig = i.Key,
                        Protocol = i.Value
                    }).ToArray()
            };

            var serviceEntries = new List<ServiceEntry>();
            foreach (var serviceConfig in descriptor.Services)
            {
                var export = await Export(serviceConfig, applicationModel);
                serviceEntries.Add(new ServiceEntry
                {
                    Exporter = export,
                    ServiceConfig = serviceConfig,
                    Protocol = applicationModel.GetProtocol(new Uri(serviceConfig.Export).Scheme).Protocol,
                    RegistryTable = applicationModel.GetRegistryTable(serviceConfig.Registry).RegistryTable
                });
            }

            var callerEntries = new List<CallerEntry>();
            foreach (var refererConfig in descriptor.Referers)
            {
                var caller = await Referer(refererConfig, applicationModel);
                callerEntries.Add(new CallerEntry
                {
                    Caller = caller,
                    Protocol = applicationModel.GetProtocol(refererConfig.Protocol).Protocol,
                    RefererConfig = refererConfig,
                    RegistryTable = applicationModel.GetRegistryTable(refererConfig.Registry).RegistryTable
                });
            }

            applicationModel.ServiceEntries = serviceEntries.ToArray();
            applicationModel.CallerEntries = callerEntries.ToArray();

            return applicationModel;
        }

        #endregion Implementation of IApplicationFactory

        public async Task<IExporter> Export(ServiceConfig config, ApplicationModel applicationModel)
        {
            var uri = new Uri(config.Export);
            var protocolName = uri.Scheme;
            var port = uri.Port;
            var host = uri.Host;

            var registryTable = applicationModel.GetRegistryTable(config.Registry).RegistryTable;
            var protocol = applicationModel.GetProtocol(protocolName).Protocol;
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

        public async Task<ICaller> Referer(RefererConfig config, ApplicationModel applicationModel)
        {
            var registryTable = applicationModel.GetRegistryTable(config.Registry).RegistryTable;
            var protocolName = config.Protocol;
            var protocol = applicationModel.GetProtocol(protocolName).Protocol;

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