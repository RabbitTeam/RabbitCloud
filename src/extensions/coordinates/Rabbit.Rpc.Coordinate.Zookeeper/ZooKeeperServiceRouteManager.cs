using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using Rabbit.Rpc.Routing;
using Rabbit.Rpc.Routing.Implementation;
using Rabbit.Rpc.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Coordinate.Zookeeper
{
    /// <summary>
    /// 基于zookeeper的服务路由管理者。
    /// </summary>
    public class ZooKeeperServiceRouteManager : ServiceRouteManagerBase, IDisposable
    {
        #region Field

        private ZooKeeper _zooKeeper;
        private readonly ZookeeperConfigInfo _configInfo;
        private readonly ISerializer<byte[]> _serializer;
        private readonly IServiceRouteFactory _serviceRouteFactory;
        private readonly ILogger<ZooKeeperServiceRouteManager> _logger;
        private ServiceRoute[] _routes;
        private readonly ManualResetEvent _connectionWait = new ManualResetEvent(false);

        #endregion Field

        #region Constructor

        public ZooKeeperServiceRouteManager(ZookeeperConfigInfo configInfo, ISerializer<byte[]> serializer,
            ISerializer<string> stringSerializer, IServiceRouteFactory serviceRouteFactory,
            ILogger<ZooKeeperServiceRouteManager> logger) : base(stringSerializer)
        {
            _configInfo = configInfo;
            _serializer = serializer;
            _serviceRouteFactory = serviceRouteFactory;
            _logger = logger;
            CreateZooKeeper().Wait();
            EnterRoutes().Wait();
        }

        #endregion Constructor

        #region Overrides of ServiceRouteManagerBase

        /// <summary>
        /// 获取所有可用的服务路由信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        public override async Task<IEnumerable<ServiceRoute>> GetRoutesAsync()
        {
            await EnterRoutes();
            return _routes;
        }

        /// <summary>
        /// 清空所有的服务路由。
        /// </summary>
        /// <returns>一个任务。</returns>
        public override async Task ClearAsync()
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("准备清空所有路由配置。");
            var path = _configInfo.RoutePath;
            var childrens = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            var index = 0;
            while (childrens.Any())
            {
                var nodePath = "/" + string.Join("/", childrens);

                if (await _zooKeeper.existsAsync(nodePath) != null)
                {
                    var result = await _zooKeeper.getChildrenAsync(nodePath);
                    if (result?.Children != null)
                    {
                        foreach (var child in result.Children)
                        {
                            var childPath = $"{nodePath}/{child}";
                            if (_logger.IsEnabled(LogLevel.Debug))
                                _logger.LogDebug($"准备删除：{childPath}。");
                            await _zooKeeper.deleteAsync(childPath);
                        }
                    }
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug($"准备删除：{nodePath}。");
                    await _zooKeeper.deleteAsync(nodePath);
                }
                index++;
                childrens = childrens.Take(childrens.Length - index).ToArray();
            }
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("路由配置清空完成。");
        }

        /// <summary>
        /// 设置服务路由。
        /// </summary>
        /// <param name="routes">服务路由集合。</param>
        /// <returns>一个任务。</returns>
        protected override async Task SetRoutesAsync(IEnumerable<ServiceRouteDescriptor> routes)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("准备添加服务路由。");
            await CreateSubdirectory(_configInfo.RoutePath);

            var path = _configInfo.RoutePath;
            if (!path.EndsWith("/"))
                path += "/";

            routes = routes.ToArray();

            if (_routes != null)
            {
                var oldRouteIds = _routes.Select(i => i.ServiceDescriptor.Id).ToArray();
                var newRouteIds = routes.Select(i => i.ServiceDescriptor.Id).ToArray();
                var deletedRouteIds = oldRouteIds.Except(newRouteIds).ToArray();
                foreach (var deletedRouteId in deletedRouteIds)
                {
                    var nodePath = $"{path}{deletedRouteId}";
                    await _zooKeeper.deleteAsync(nodePath);
                }
            }

            foreach (var serviceRoute in routes)
            {
                var nodePath = $"{path}{serviceRoute.ServiceDescriptor.Id}";
                var nodeData = _serializer.Serialize(serviceRoute);
                if (await _zooKeeper.existsAsync(nodePath) == null)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug($"节点：{nodePath}不存在将进行创建。");

                    await _zooKeeper.createAsync(nodePath, nodeData, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                }
                else
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug($"将更新节点：{nodePath}的数据。");

                    var onlineData = (await _zooKeeper.getDataAsync(nodePath)).Data;
                    if (!DataEquals(nodeData, onlineData))
                        await _zooKeeper.setDataAsync(nodePath, nodeData);
                }
            }
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("服务路由添加成功。");
        }

        #endregion Overrides of ServiceRouteManagerBase

        #region Private Method

        private async Task CreateZooKeeper()
        {
            if (_zooKeeper != null)
                await _zooKeeper.closeAsync();
            _zooKeeper = new ZooKeeper(_configInfo.ConnectionString, (int)_configInfo.SessionTimeout.TotalMilliseconds
                , new ReconnectionWatcher(
                    () =>
                    {
                        _connectionWait.Set();
                    },
                    async () =>
                    {
                        _connectionWait.Reset();
                        await CreateZooKeeper();
                    }));
        }

        private async Task CreateSubdirectory(string path)
        {
            _connectionWait.WaitOne();
            if (await _zooKeeper.existsAsync(path) != null)
                return;

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation($"节点{path}不存在，将进行创建。");

            var childrens = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var nodePath = "/";

            foreach (var children in childrens)
            {
                nodePath += children;
                if (await _zooKeeper.existsAsync(nodePath) == null)
                {
                    await _zooKeeper.createAsync(nodePath, null, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                }
                nodePath += "/";
            }
        }

        private async Task<ServiceRoute> GetRoute(byte[] data)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"准备转换服务路由，配置内容：{Encoding.UTF8.GetString(data)}。");

            if (data == null)
                return null;

            var descriptor = _serializer.Deserialize<byte[], ServiceRouteDescriptor>(data);

            return (await _serviceRouteFactory.CreateServiceRoutesAsync(new[] { descriptor })).First();
        }

        private async Task<ServiceRoute> GetRoute(string path)
        {
            var watcher = new NodeMonitorWatcher(_zooKeeper, path,
                async (oldData, newData) => await NodeChange(oldData, newData));
            var data = (await _zooKeeper.getDataAsync(path, watcher)).Data;
            watcher.SetCurrentData(data);
            return await GetRoute(data);
        }

        private async Task<ServiceRoute[]> GetRoutes(IEnumerable<string> childrens)
        {
            var rootPath = _configInfo.RoutePath;
            if (!rootPath.EndsWith("/"))
                rootPath += "/";

            childrens = childrens.ToArray();
            var routes = new List<ServiceRoute>(childrens.Count());

            foreach (var children in childrens)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"准备从节点：{children}中获取路由信息。");

                var nodePath = $"{rootPath}{children}";
                routes.Add(await GetRoute(nodePath));
            }

            return routes.ToArray();
        }

        private async Task EnterRoutes()
        {
            if (_routes != null)
                return;
            _connectionWait.WaitOne();

            var watcher = new ChildrenMonitorWatcher(_zooKeeper, _configInfo.RoutePath,
                async (oldChildrens, newChildrens) => await ChildrenChange(oldChildrens, newChildrens));
            if (await _zooKeeper.existsAsync(_configInfo.RoutePath, watcher) != null)
            {
                var result = await _zooKeeper.getChildrenAsync(_configInfo.RoutePath, watcher);
                var childrens = result.Children.ToArray();
                watcher.SetCurrentData(childrens);
                _routes = await GetRoutes(childrens);
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning($"无法获取路由信息，因为节点：{_configInfo.RoutePath}，不存在。");
                _routes = new ServiceRoute[0];
            }
        }

        private static bool DataEquals(IReadOnlyList<byte> data1, IReadOnlyList<byte> data2)
        {
            if (data1.Count != data2.Count)
                return false;
            for (var i = 0; i < data1.Count; i++)
            {
                var b1 = data1[i];
                var b2 = data2[i];
                if (b1 != b2)
                    return false;
            }
            return true;
        }

        public async Task NodeChange(byte[] oldData, byte[] newData)
        {
            if (DataEquals(oldData, newData))
                return;

            var newRoute = await GetRoute(newData);
            //得到旧的路由。
            var oldRoute = _routes.First(i => i.ServiceDescriptor.Id == newRoute.ServiceDescriptor.Id);

            lock (_routes)
            {
                //删除旧路由，并添加上新的路由。
                _routes =
                    _routes
                        .Where(i => i.ServiceDescriptor.Id != newRoute.ServiceDescriptor.Id)
                        .Concat(new[] { newRoute }).ToArray();
            }
            //触发路由变更事件。
            OnChanged(new ServiceRouteChangedEventArgs(newRoute, oldRoute));
        }

        public async Task ChildrenChange(string[] oldChildrens, string[] newChildrens)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"最新的节点信息：{string.Join(",", newChildrens)}");

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"旧的节点信息：{string.Join(",", oldChildrens)}");

            //计算出已被删除的节点。
            var deletedChildrens = oldChildrens.Except(newChildrens).ToArray();
            //计算出新增的节点。
            var createdChildrens = newChildrens.Except(oldChildrens).ToArray();

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation($"需要被删除的路由节点：{string.Join(",", deletedChildrens)}");
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation($"需要被添加的路由节点：{string.Join(",", createdChildrens)}");

            //获取新增的路由信息。
            var newRoutes = (await GetRoutes(createdChildrens)).ToArray();

            var routes = _routes.ToArray();
            lock (_routes)
            {
                _routes = _routes
                    //删除无效的节点路由。
                    .Where(i => !deletedChildrens.Contains(i.ServiceDescriptor.Id))
                    //连接上新的路由。
                    .Concat(newRoutes)
                    .ToArray();
            }
            //需要删除的路由集合。
            var deletedRoutes = routes.Where(i => deletedChildrens.Contains(i.ServiceDescriptor.Id)).ToArray();
            //触发删除事件。
            OnRemoved(deletedRoutes.Select(route => new ServiceRouteEventArgs(route)).ToArray());

            //触发路由被创建事件。
            OnCreated(newRoutes.Select(route => new ServiceRouteEventArgs(route)).ToArray());

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("路由数据更新成功。");
        }

        #endregion Private Method

        #region Watcher Class

        protected class ReconnectionWatcher : Watcher
        {
            private readonly Action _connectioned;
            private readonly Action _disconnect;

            public ReconnectionWatcher(Action connectioned, Action disconnect)
            {
                _connectioned = connectioned;
                _disconnect = disconnect;
            }

            #region Overrides of Watcher

            /// <summary>Processes the specified event.</summary>
            /// <param name="watchedEvent">The event.</param>
            /// <returns></returns>
            public override async Task process(WatchedEvent watchedEvent)
            {
                if (watchedEvent.getState() == Event.KeeperState.SyncConnected)
                {
                    _connectioned();
                }
                else
                {
                    _disconnect();
                }
#if NET
                await Task.FromResult(1);
#else
                await Task.CompletedTask;
#endif
            }

            #endregion Overrides of Watcher
        }

        protected abstract class WatcherBase : Watcher
        {
            protected string Path { get; }

            protected WatcherBase(string path)
            {
                Path = path;
            }

            public override async Task process(WatchedEvent watchedEvent)
            {
                if (watchedEvent.getState() != Event.KeeperState.SyncConnected || watchedEvent.getPath() != Path)
                    return;
                await ProcessImpl(watchedEvent);
            }

            protected abstract Task ProcessImpl(WatchedEvent watchedEvent);
        }

        protected class NodeMonitorWatcher : WatcherBase
        {
            private readonly ZooKeeper _zooKeeper;
            private readonly Action<byte[], byte[]> _action;
            private byte[] _currentData;

            public NodeMonitorWatcher(ZooKeeper zooKeeper, string path, Action<byte[], byte[]> action) : base(path)
            {
                _zooKeeper = zooKeeper;
                _action = action;
            }

            public NodeMonitorWatcher SetCurrentData(byte[] currentData)
            {
                _currentData = currentData;

                return this;
            }

            #region Overrides of WatcherBase

            protected override async Task ProcessImpl(WatchedEvent watchedEvent)
            {
                var path = Path;
                switch (watchedEvent.get_Type())
                {
                    case Event.EventType.NodeDataChanged:
                        var watcher = new NodeMonitorWatcher(_zooKeeper, path, _action);
                        var data = await _zooKeeper.getDataAsync(path, watcher);
                        var newData = data.Data;
                        _action(_currentData, newData);
                        watcher.SetCurrentData(newData);
                        break;

                        /*case Event.EventType.NodeDeleted:
                                _action(_currentData, null);
                                break;*/
                }
            }

            #endregion Overrides of WatcherBase
        }

        protected class ChildrenMonitorWatcher : WatcherBase
        {
            private readonly ZooKeeper _zooKeeper;
            private readonly Action<string[], string[]> _action;
            private string[] _currentData = new string[0];

            public ChildrenMonitorWatcher(ZooKeeper zooKeeper, string path, Action<string[], string[]> action)
                : base(path)
            {
                _zooKeeper = zooKeeper;
                _action = action;
            }

            public ChildrenMonitorWatcher SetCurrentData(string[] currentData)
            {
                _currentData = currentData ?? new string[0];

                return this;
            }

            #region Overrides of WatcherBase

            protected override async Task ProcessImpl(WatchedEvent watchedEvent)
            {
                var path = Path;
                Func<ChildrenMonitorWatcher> getWatcher = () => new ChildrenMonitorWatcher(_zooKeeper, path, _action);
                switch (watchedEvent.get_Type())
                {
                    //创建之后开始监视下面的子节点情况。
                    case Event.EventType.NodeCreated:
                        await _zooKeeper.getChildrenAsync(path, getWatcher());
                        break;

                    //子节点修改则继续监控子节点信息并通知客户端数据变更。
                    case Event.EventType.NodeChildrenChanged:
                        try
                        {
                            var watcher = getWatcher();
                            var result = await _zooKeeper.getChildrenAsync(path, watcher);
                            var childrens = result.Children.ToArray();
                            _action(_currentData, childrens);
                            watcher.SetCurrentData(childrens);
                        }
                        catch (KeeperException.NoNodeException)
                        {
                            _action(_currentData, new string[0]);
                        }
                        break;

                    //删除之后开始监控自身节点，并通知客户端数据被清空。
                    case Event.EventType.NodeDeleted:
                        {
                            var watcher = getWatcher();
                            await _zooKeeper.existsAsync(path, watcher);
                            _action(_currentData, new string[0]);
                            watcher.SetCurrentData(new string[0]);
                        }
                        break;
                }
            }

            #endregion Overrides of WatcherBase
        }

        #endregion Watcher Class

        #region Help Class

        /// <summary>
        /// zookeeper连接信息。
        /// </summary>
        public class ZookeeperConfigInfo
        {
            /// <summary>
            /// 初始化一个会话超时为20秒的Zookeeper连接信息。
            /// </summary>
            /// <param name="connectionString">连接字符串。</param>
            /// <param name="routePath">路由配置路径。</param>
            /// <param name="chRoot">根节点。</param>
            public ZookeeperConfigInfo(string connectionString, string routePath = "/dotnet/serviceRoutes", string chRoot = null) : this(connectionString, TimeSpan.FromSeconds(20), routePath, chRoot)
            {
            }

            /// <summary>
            /// 初始化一个新的Zookeeper连接信息。
            /// </summary>
            /// <param name="connectionString">连接字符串。</param>
            /// <param name="routePath">路由配置路径。</param>
            /// <param name="sessionTimeout">会话超时时间。</param>
            /// <param name="chRoot">根节点。</param>
            public ZookeeperConfigInfo(string connectionString, TimeSpan sessionTimeout, string routePath = "/dotnet/serviceRoutes", string chRoot = null)
            {
                ChRoot = chRoot;
                ConnectionString = connectionString;
                RoutePath = routePath;
                SessionTimeout = sessionTimeout;
            }

            /// <summary>
            /// 连接字符串。
            /// </summary>
            public string ConnectionString { get; set; }

            /// <summary>
            /// 路由配置路径。
            /// </summary>
            public string RoutePath { get; set; }

            /// <summary>
            /// 会话超时时间。
            /// </summary>
            public TimeSpan SessionTimeout { get; set; }

            /// <summary>
            /// 根节点。
            /// </summary>
            public string ChRoot { get; set; }
        }

        #endregion Help Class

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _connectionWait.Dispose();
            _zooKeeper.closeAsync().Wait();
        }

        #endregion Implementation of IDisposable
    }
}