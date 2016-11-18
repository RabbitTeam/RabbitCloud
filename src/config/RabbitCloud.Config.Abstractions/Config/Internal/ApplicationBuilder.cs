using Microsoft.Extensions.DependencyInjection;
using RabbitCloud.Abstractions;
using RabbitCloud.Config.Abstractions.Internal;
using RabbitCloud.Registry.Abstractions;
using RabbitCloud.Registry.Abstractions.Cluster;
using RabbitCloud.Rpc.Abstractions.Internal;
using RabbitCloud.Rpc.Abstractions.Protocol;
using RabbitCloud.Rpc.Abstractions.Proxy;
using RabbitCloud.Rpc.Cluster.Internal.Available;
using RabbitCloud.Rpc.Cluster.LoadBalance;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitCloud.Config.Abstractions.Config.Internal
{
    public class ApplicationBuilder : IApplicationBuilder
    {
        private readonly IServiceProvider _services;
        private readonly ApplicationConfig _applicationConfig;

        public ApplicationBuilder(IServiceProvider services, ApplicationConfig applicationConfig)
        {
            _services = services;
            _applicationConfig = applicationConfig;
        }

        #region Implementation of IApplicationBuilder

        public async Task<ApplicationEntry> Build()
        {
            var application = new ApplicationEntry
            {
                Name = _applicationConfig.Name
            };
            var references = new List<ReferenceEntry>();
            var services = new List<ServiceEntry>();

            foreach (var config in _applicationConfig.ServiceExportConfigs)
            {
                var entry = await GetServiceEntry(config);
                services.Add(entry);
            }
            foreach (var config in _applicationConfig.ReferenceConfigs)
            {
                var entry = await GetReferenceEntry(config);
                references.Add(entry);
            }

            application.References = references.ToArray();
            application.Services = services.ToArray();
            return application;
        }

        #endregion Implementation of IApplicationBuilder

        protected async Task<ServiceEntry> GetServiceEntry(ServiceExportConfig config)
        {
            var registry = await GetRegistry(config.RegistryConfig);
            var protocol = await GetRegistryProtocol(registry, await GetProtocol(config.ProtocolConfig));
            var serviceFactory = await GetServiceFactory(config.ServiceConfig);
            var entry = new ServiceEntry
            {
                Protocol = protocol,
                Registry = registry,
                ServiceType = Type.GetType(config.ServiceConfig.Type),
                ServiceFactory = serviceFactory
            };

            await protocol.Export(new DefaultProvider(entry.ServiceFactory, new Url($"{config.ProtocolConfig.Name}://{config.ProtocolConfig.Host}:{config.ProtocolConfig.Port}/{entry.ServiceType.Name}"), entry.ServiceType));

            return entry;
        }

        protected async Task<ReferenceEntry> GetReferenceEntry(ReferenceConfig config)
        {
            var registry = await GetRegistry(config.RegistryConfig);
            var protocol = await GetRegistryProtocol(registry, await GetProtocol(config.ProtocolConfig));
            var serviceType = Type.GetType(config.InterfaceType);
            var serviceProxy = await GetServiceProxy(protocol, serviceType);
            var entry = new ReferenceEntry
            {
                Protocol = protocol,
                Registry = registry,
                ServiceProxy = serviceProxy,
                Config = config
            };

            return entry;
        }

        protected static Task<IProtocol> GetRegistryProtocol(IRegistry registry, IProtocol protocol)
        {
            return Task.FromResult<IProtocol>(new RegistryProtocol(registry, protocol, new AvailableCluster(new RoundRobinLoadBalance())));
        }

        private async Task<IProtocol> GetProtocol(ProtocolConfig config)
        {
            var locator = _services.GetRequiredService<IProtocolLocator>();
            var instance = await locator.GetProtocol(config.Name);
            return instance;
        }

        private async Task<IRegistry> GetRegistry(RegistryConfig config)
        {
            var locator = _services.GetRequiredService<IRegistryLocator>();
            var instance = await locator.GetRegistry(new Url(config.Address));
            return instance;
        }

        private Task<Func<object>> GetServiceFactory(ServiceConfig config)
        {
            return Task.FromResult<Func<object>>(() => _services.GetRequiredService(Type.GetType(config.Type)));
        }

        private async Task<object> GetServiceProxy(IProtocol protocol, Type serviceType)
        {
            var proxyFactory = _services.GetRequiredService<IProxyFactory>();
            var referer = await protocol.Refer(serviceType, new Url($"registry://temp/{serviceType.Name}"));
            var serviceProxy = proxyFactory.GetProxy(serviceType, new RefererInvocationHandler(referer).Invoke);
            return serviceProxy;
        }
    }
}