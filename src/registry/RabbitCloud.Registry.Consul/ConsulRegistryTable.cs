using Consul;
using Microsoft.Extensions.Logging;
using RabbitCloud.Abstractions.Logging;
using RabbitCloud.Registry.Abstractions;
using RabbitCloud.Registry.Consul.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace RabbitCloud.Registry.Consul
{
    public class ConsulRegistryTable : IRegistryTable, IDisposable
    {
        #region Field

        private readonly ILogger<ConsulRegistryTable> _logger;
        private readonly ConsulClient _consulClient;
        private readonly IList<ServiceRegistryDescriptor> _registeredServices = new List<ServiceRegistryDescriptor>();

        private readonly ConcurrentDictionary<ServiceRegistryDescriptor, NotifyDelegate> _notifyDelegates = new ConcurrentDictionary<ServiceRegistryDescriptor, NotifyDelegate>();
        private readonly HeartbeatManager _heartbeatManager;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly ConcurrentDictionary<string, IList<ServiceRegistryDescriptor>> _serviceRegistryDescriptorDictionary = new ConcurrentDictionary<string, IList<ServiceRegistryDescriptor>>();

        #endregion Field

        #region Constructor

        public ConsulRegistryTable(Uri url, ILogger<ConsulRegistryTable> logger = null)
        {
            _logger = logger ?? NullLogger<ConsulRegistryTable>.Instance;
            _consulClient = new ConsulClient(config =>
            {
                config.Address = url;
            });

            _heartbeatManager = new HeartbeatManager(_consulClient);
        }

        #endregion Constructor

        #region Implementation of IRegistryService

        public async Task RegisterAsync(ServiceRegistryDescriptor descriptor)
        {
            var registration = ConsulUtils.GetServiceRegistration(descriptor);

            //注册服务
            var result = await _consulClient.Agent.ServiceRegister(registration, _cancellationTokenSource.Token);

            //添加到心跳管理
            _heartbeatManager.AddHeartbeat(registration.ID);

            if (result.StatusCode != HttpStatusCode.OK && _logger.IsEnabled(LogLevel.Error))
                _logger.LogError($"ServiceRegister is return code :{result.StatusCode}");

            _registeredServices.Add(descriptor);
        }

        public async Task UnRegisterAsync(ServiceRegistryDescriptor descriptor)
        {
            var serviceId = ConsulUtils.GetServiceId(descriptor);

            //注销服务
            var result =
                await _consulClient.Agent.ServiceDeregister(serviceId, _cancellationTokenSource.Token);

            //从心跳管理中移除
            _heartbeatManager.RemoveHeartbeat(serviceId);

            if (result.StatusCode != HttpStatusCode.OK && _logger.IsEnabled(LogLevel.Error))
                _logger.LogError($"ServiceDeregister is return code :{result.StatusCode}");
        }

        public async Task SetAvailableAsync(ServiceRegistryDescriptor descriptor)
        {
            var serviceId = ConsulUtils.GetServiceId(descriptor);
            await _consulClient.Agent.PassTTL("service:" + serviceId, await _consulClient.Agent.GetNodeName(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
            _heartbeatManager.RemoveHeartbeat(serviceId);
        }

        public async Task SetUnAvailableAsync(ServiceRegistryDescriptor descriptor)
        {
            var serviceId = ConsulUtils.GetServiceId(descriptor);
            await _consulClient.Agent.FailTTL("service:" + serviceId, await _consulClient.Agent.GetNodeName(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
            _heartbeatManager.RemoveHeartbeat(serviceId);
        }

        public IReadOnlyCollection<ServiceRegistryDescriptor> GetRegisteredServices()
        {
            return _registeredServices.ToArray();
        }

        #endregion Implementation of IRegistryService

        #region Implementation of IDiscoveryService

        public void Subscribe(ServiceRegistryDescriptor descriptor, NotifyDelegate listener)
        {
            Task.Run(async () => await Discover(descriptor)).Wait();

            if (_notifyDelegates.TryGetValue(descriptor, out NotifyDelegate value))
                listener = value + listener;

            _notifyDelegates.AddOrUpdate(descriptor, listener, (s, ss) => listener);
        }

        public void UnSubscribe(ServiceRegistryDescriptor descriptor, NotifyDelegate listener)
        {
            if (_notifyDelegates.TryGetValue(descriptor, out NotifyDelegate value))
                listener = value - listener;

            _notifyDelegates.TryUpdate(descriptor, listener, value);
        }

        public async Task<IReadOnlyCollection<ServiceRegistryDescriptor>> Discover(ServiceRegistryDescriptor descriptor)
        {
            //如果已经load过则直接返回
            if (_serviceRegistryDescriptorDictionary.TryGetValue(descriptor.ServiceKey.Group, out IList<ServiceRegistryDescriptor> descriptors))
                return descriptors.ToArray();

            //从consul中加载
            descriptors = (await DiscoverByConsul(descriptor)).ToList();

            //添加到本地
            if (_serviceRegistryDescriptorDictionary.TryAdd(descriptor.ServiceKey.Group, descriptors))
            {
                //添加订阅事件用于更新本地服务信息
                Subscribe(descriptor, (currentDescriptor, newDescriptors) =>
                {
                    var serviceName = currentDescriptor.ServiceKey.Group;
                    var newDescriptorList = newDescriptors.ToList();
                    _serviceRegistryDescriptorDictionary.AddOrUpdate(serviceName, newDescriptorList, (d, ds) => newDescriptorList);
                });
            }

            return descriptors.ToArray();
        }

        #endregion Implementation of IDiscoveryService

        #region Private Method

        private void Notify(ServiceRegistryDescriptor registryDescriptor, IReadOnlyCollection<ServiceRegistryDescriptor> descriptors)
        {
            foreach (var notifyDelegatesValue in _notifyDelegates.Values)
            {
                notifyDelegatesValue(registryDescriptor, descriptors);
            }
        }

        private async Task<IEnumerable<ServiceRegistryDescriptor>> DiscoverByConsul(ServiceRegistryDescriptor descriptor)
        {
            //获取服务信息
            var result = await _consulClient.Health.Service(ConsulUtils.GetConsulServiceName(descriptor), null, false, _cancellationTokenSource.Token);

            await Task.Factory.StartNew(async () =>
            {
                //监听变化
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    var waitIndex = result.LastIndex;
                    result = await _consulClient.Health.Service(ConsulUtils.GetConsulServiceName(descriptor), null, false, new QueryOptions
                    {
                        WaitIndex = waitIndex
                    }, _cancellationTokenSource.Token);

                    //如果发生变更则处理通知
                    if (result.LastIndex != waitIndex && waitIndex != 0)
                        Notify(descriptor, ConsulUtils.GetServiceRegistryDescriptors(result.Response).ToArray());
                }
            }, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            return ConsulUtils.GetServiceRegistryDescriptors(result.Response).ToList();
        }

        public bool IsAvailable(ServiceEntry serviceEntry)
        {
            return serviceEntry.Checks.All(IsAvailable);
        }

        public bool IsAvailable(HealthCheck healthCheck)
        {
            return HealthStatus.Passing.Equals(healthCheck.Status);
        }

        #endregion Private Method

        #region IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _consulClient?.Dispose();
            _heartbeatManager?.Dispose();
            _cancellationTokenSource?.Dispose();
        }

        #endregion IDisposable
    }
}