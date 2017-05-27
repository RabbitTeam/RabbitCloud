using Microsoft.Extensions.DependencyInjection;
using RabbitCloud.Abstractions;
using RabbitCloud.Config.Abstractions;
using RabbitCloud.Config.Abstractions.Support;
using RabbitCloud.Registry.Abstractions;
using RabbitCloud.Rpc;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RabbitCloud.Config.Internal
{
    public class DefaultApplicationFactory : IApplicationFactory
    {
        private readonly IRegistryTableFactory _registryTableFactory;
        private readonly IProtocolFactory _protocolFactory;
        private readonly IProxyFactory _proxyFactory;
        private readonly IClusterFactory _clusterFactory;

        public DefaultApplicationFactory(IRegistryTableFactory registryTableFactory, IProtocolFactory protocolFactory, IProxyFactory proxyFactory, IClusterFactory clusterFactory)
        {
            _registryTableFactory = registryTableFactory;
            _protocolFactory = protocolFactory;
            _proxyFactory = proxyFactory;
            _clusterFactory = clusterFactory;
        }

        #region Implementation of IApplicationFactory

        public async Task<IApplicationModel> CreateApplicationAsync(ApplicationModelDescriptor descriptor)
        {
            var applicationModel = new ApplicationModel(_proxyFactory);

            HandleRegistryTableEntries(applicationModel, descriptor);
            HandleProtocolEntries(applicationModel, descriptor);
            await HandleServiceEntries(applicationModel, descriptor);
            await HandleCallerEntries(applicationModel, descriptor);

            return applicationModel;
        }

        #endregion Implementation of IApplicationFactory

        #region Private Method

        private void HandleRegistryTableEntries(ApplicationModel applicationModel, ApplicationModelDescriptor descriptor)
        {
            applicationModel.RegistryTables = descriptor.Registrys
                .ToDictionary(i => i, _registryTableFactory.GetRegistryTable).Select(
                    i => new RegistryTableEntry
                    {
                        RegistryConfig = i.Key,
                        RegistryTable = i.Value
                    }).ToArray();
        }

        private void HandleProtocolEntries(ApplicationModel applicationModel, ApplicationModelDescriptor descriptor)
        {
            applicationModel.Protocols = descriptor.Protocols.ToDictionary(i => i, _protocolFactory.GetProtocol).Select(
                i => new ProtocolEntry
                {
                    ProtocolConfig = i.Key,
                    Protocol = i.Value
                }).ToArray();
        }

        private static async Task HandleServiceEntries(ApplicationModel applicationModel, ApplicationModelDescriptor descriptor)
        {
            if (descriptor.Services == null)
                return;
            var serviceCollection = new ServiceCollection();

            foreach (var serviceConfig in descriptor.Services)
            {
                var serviceType = Type.GetType(serviceConfig.Interface);
                var implementType = Type.GetType(serviceConfig.Implement);

                serviceCollection.AddSingleton(serviceType, implementType);
            }

            var serviceContainer = serviceCollection.BuildServiceProvider();

            var serviceEntries = new List<ServiceEntry>();
            foreach (var serviceConfig in descriptor.Services)
            {
                var exportItem = ResolveExport(serviceConfig.Export);
                var export = await Export(serviceConfig, exportItem, applicationModel, type => serviceContainer.GetService(type));
                serviceEntries.Add(new ServiceEntry
                {
                    Exporter = export,
                    ServiceConfig = serviceConfig,
                    Protocol = applicationModel.GetProtocolEntry(exportItem.protocol).Protocol,
                    RegistryTable = applicationModel.GetRegistryTableEntry(serviceConfig.Registry).RegistryTable
                });
            }
            applicationModel.ServiceEntries = serviceEntries.ToArray();
        }

        private async Task HandleCallerEntries(ApplicationModel applicationModel, ApplicationModelDescriptor descriptor)
        {
            if (descriptor.Referers == null)
                return;

            var callerEntries = new List<CallerEntry>();
            foreach (var refererConfig in descriptor.Referers)
            {
                var caller = await Referer(refererConfig, applicationModel);
                callerEntries.Add(new CallerEntry
                {
                    Caller = caller,
                    Protocol = applicationModel.GetProtocolEntry(refererConfig.Protocol).Protocol,
                    RefererConfig = refererConfig,
                    RegistryTable = applicationModel.GetRegistryTableEntry(refererConfig.Registry).RegistryTable
                });
            }

            applicationModel.CallerEntries = callerEntries.ToArray();
        }

        private static async Task<IExporter> Export(ServiceConfig config, (string protocol, string host, int? port) exportItem, ApplicationModel applicationModel, Func<Type, object> instanceFactory)
        {
            var protocolId = exportItem.protocol;
            var port = exportItem.port;
            var host = exportItem.host;

            var registryTable = applicationModel.GetRegistryTableEntry(config.Registry).RegistryTable;
            var protocolEntry = applicationModel.GetProtocolEntry(protocolId);
            var protocol = protocolEntry.Protocol;
            var serviceType = Type.GetType(config.Interface);

            if (string.IsNullOrEmpty(config.Id))
                config.Id = serviceType.Name;

            var export = protocol.Export(new ExportContext
            {
                Caller = new TypeCaller(serviceType, () => instanceFactory(serviceType)),
                EndPoint = GetIpEndPoint(host, port),
                ServiceKey = new ServiceKey(config.Group, config.Id, "1.0.0")
            });

            await registryTable.RegisterAsync(new ServiceRegistryDescriptor
            {
                Host = host,
                Port = port ?? 0,
                Protocol = protocolEntry.ProtocolConfig.Name,
                ServiceKey = new ServiceKey(config.Group, config.Id, "1.0.0")
            });

            return export;
        }

        private async Task<ICaller> Referer(RefererConfig config, IApplicationModel applicationModel)
        {
            var registryTable = applicationModel.GetRegistryTableEntry(config.Registry).RegistryTable;

            var serviceType = Type.GetType(config.Interface);
            if (string.IsNullOrEmpty(config.Id))
                config.Id = serviceType.Name;

            var serviceKey = new ServiceKey(config.Group, config.Id, "1.0.0");
            var descriptors = await registryTable.Discover(serviceKey);

            var referers = GetCallers(applicationModel, descriptors);

            var cluster = _clusterFactory.CreateCluster(referers, config.Cluster, config.LoadBalance, config.HaStrategy);

            registryTable.Subscribe(serviceKey, (currentServiceKey, newDescriptors) =>
            {
                cluster.Callers = newDescriptors == null ? null : GetCallers(applicationModel, newDescriptors).ToArray();
            });

            return cluster;
        }

        private static IEnumerable<ICaller> GetCallers(IApplicationModel applicationModel, IEnumerable<ServiceRegistryDescriptor> descriptors)
        {
            return descriptors.Select(descriptor =>
            {
                var protocol = applicationModel.GetProtocol(descriptor.Protocol);
                return protocol.Refer(new ReferContext
                {
                    EndPoint = GetIpEndPoint(descriptor.Host, descriptor.Port),
                    ServiceKey = descriptor.ServiceKey
                });
            });
        }

        private static (string protocol, string host, int? port) ResolveExport(string export)
        {
            if (Uri.TryCreate(export, UriKind.Absolute, out Uri uri))
            {
                return (uri.Scheme, uri.Host, uri.Port);
            }
            var temp = export.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim('/')).ToArray();

            if (!temp.Any())
                return (null, null, null);

            int? GetPort(string str)
            {
                return int.TryParse(str, out int value) ? (int?)value : null;
            }

            switch (temp.Length)
            {
                case 1:
                    return (temp[0], IPAddress.Loopback.ToString(), null);

                case 2:
                    {
                        var port = GetPort(temp[1]);
                        var isPort = port.HasValue;
                        return (temp[0], (isPort ? IPAddress.Loopback.ToString() : temp[1]), port);
                    }

                default:
                    return (temp[0], temp[1], GetPort(temp[3]));
            }
        }

        private static IPEndPoint GetIpEndPoint(string host, int? port)
        {
            return !IPAddress.TryParse(host, out IPAddress address) ? null : new IPEndPoint(address, port ?? 0);
        }

        #endregion Private Method
    }
}