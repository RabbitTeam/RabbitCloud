using Org.Apache.Zookeeper.Data;
using Rabbit.Rpc.Address;
using Rabbit.Rpc.Client.Routing;
using Rabbit.Rpc.Routing;
using Rabbit.Rpc.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZooKeeperNet;

namespace Rabbit.Rpc.Coordinate.Zookeeper
{
    /// <summary>
    /// 基于Zookeeper的服务路由提供程序。
    /// </summary>
    public class ZookeeperServiceRouteProvider : IServiceRouteProvider, IDisposable
    {
        #region Field

        private ZooKeeper _zooKeeper;
        private readonly ZookeeperConfigInfo _configInfo;
        private readonly ISerializer _serializer;
        private IEnumerable<ServiceRoute> _routes;
        private readonly ManualResetEvent _connectionWait = new ManualResetEvent(false);

        #endregion Field

        public ZookeeperServiceRouteProvider(ZookeeperConfigInfo configInfo, ISerializer serializer)
        {
            _configInfo = configInfo;
            _serializer = serializer;
            CreateZooKeeper();
            EnterRoutes();
        }

        #region Implementation of IServiceRouteProvider

        /// <summary>
        /// 获取服务路由集合。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        public Task<IEnumerable<ServiceRoute>> GetRoutesAsync()
        {
            EnterRoutes();
            return Task.FromResult(_routes);
        }

        #endregion Implementation of IServiceRouteProvider

        #region Private Method

        private void CreateZooKeeper()
        {
            _zooKeeper?.Dispose();
            _zooKeeper = new ZooKeeper(_configInfo.ConnectionString, _configInfo.SessionTimeout
                , new ReconnectionWatcher(
                () =>
                {
                    _connectionWait.Set();
                },
                () =>
                {
                    _connectionWait.Reset();
                    CreateZooKeeper();
                }));
        }

        private IEnumerable<ServiceRoute> GetRoutes(byte[] data)
        {
            if (data == null)
                return Enumerable.Empty<ServiceRoute>();

            var content = Encoding.UTF8.GetString(data);
            var config = _serializer.Deserialize<ConfigModel>(content);

            return config.Routes.Select(r => new ServiceRoute
            {
                Address = r.Address,
                ServiceDescriptor = r.ServiceDescriptor
            }).ToArray();
        }

        private void EnterRoutes()
        {
            if (_routes != null)
                return;
            _connectionWait.WaitOne();
            var watcher = new MonitorWatcher(_zooKeeper, _configInfo.RoutePath, newData =>
               {
                   _routes = GetRoutes(newData);
               });
            if (_zooKeeper.Exists(_configInfo.RoutePath, watcher) != null)
            {
                var data = _zooKeeper.GetData(_configInfo.RoutePath, watcher, new Stat());
                _routes = GetRoutes(data);
            }
            else
                _routes = Enumerable.Empty<ServiceRoute>();
        }

        #endregion Private Method

        #region Watcher Class

        protected class ReconnectionWatcher : IWatcher
        {
            private readonly Action _connectioned;
            private readonly Action _disconnect;

            public ReconnectionWatcher(Action connectioned, Action disconnect)
            {
                _connectioned = connectioned;
                _disconnect = disconnect;
            }

            #region Implementation of IWatcher

            public void Process(WatchedEvent @event)
            {
                if (@event.State == KeeperState.SyncConnected)
                {
                    _connectioned();
                }
                else
                {
                    _disconnect();
                }
            }

            #endregion Implementation of IWatcher
        }

        protected class MonitorWatcher : IWatcher
        {
            private readonly ZooKeeper _zooKeeper;
            private readonly string _path;
            private readonly Action<byte[]> _action;

            public MonitorWatcher(ZooKeeper zooKeeper, string path, Action<byte[]> action)
            {
                _zooKeeper = zooKeeper;
                _path = path;
                _action = action;
            }

            #region Implementation of IWatcher

            public void Process(WatchedEvent watchedEvent)
            {
                if (watchedEvent.State != KeeperState.SyncConnected || watchedEvent.Path != _path)
                    return;

                var watcher = new MonitorWatcher(_zooKeeper, _path, _action);
                switch (watchedEvent.Type)
                {
                    case EventType.NodeCreated:
                    case EventType.NodeDataChanged:
                        var data = _zooKeeper.GetData(_path, watcher, new Stat());
                        _action(data);
                        break;

                    case EventType.NodeDeleted:
                        _zooKeeper.Exists(_path, watcher);
                        _action(null);
                        break;
                }
            }

            #endregion Implementation of IWatcher
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

        protected class ConfigModel
        {
            public IpAddressDescriptor[] Routes { get; set; }
        }

        protected class IpAddressDescriptor
        {
            public List<IpAddressModel> Address { get; set; }
            public ServiceDescriptor ServiceDescriptor { get; set; }
        }

        #endregion Help Class

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _connectionWait.Dispose();
            _zooKeeper.Dispose();
        }

        #endregion Implementation of IDisposable
    }
}