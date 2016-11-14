using org.apache.zookeeper;
using RabbitCloud.Abstractions;
using RabbitCloud.Registry.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RabbitCloud.Registry.ZooKeeper
{
    public class ZookeeperRegistry : IRegistry
    {
        private readonly ConcurrentBag<Url> _registeredServiceUrls = new ConcurrentBag<Url>();
        private readonly org.apache.zookeeper.ZooKeeper _zooKeeper;

        public ZookeeperRegistry(org.apache.zookeeper.ZooKeeper zooKeeper)
        {
            _zooKeeper = zooKeeper;
        }

        #region Implementation of IRegistryService

        /// <summary>
        /// 注册一个服务。
        /// </summary>
        /// <param name="url">服务Url。</param>
        /// <returns>一个任务。</returns>
        public async Task Register(Url url)
        {
            await RemoveNode(url, ZkNodeType.AvailableServer);
            await RemoveNode(url, ZkNodeType.UnavailableServer);
            await CreateNode(url, ZkNodeType.AvailableServer);
        }

        /// <summary>
        /// 取消注册一个服务。
        /// </summary>
        /// <param name="url">服务Url。</param>
        /// <returns>一个任务。</returns>
        public async Task UnRegister(Url url)
        {
            await RemoveNode(url, ZkNodeType.AvailableServer);
            await RemoveNode(url, ZkNodeType.UnavailableServer);
        }

        /// <summary>
        /// 获取已经注册的所有服务Url。
        /// </summary>
        /// <returns>服务Url集合。</returns>
        public Task<Url[]> GetRegisteredServiceUrls()
        {
            return Task.FromResult(_registeredServiceUrls.ToArray());
        }

        #endregion Implementation of IRegistryService

        #region Implementation of IDiscoveryService

        /// <summary>
        /// 订阅一个注册中心。
        /// </summary>
        /// <param name="url">注册中心url。</param>
        /// <param name="listener">监听器。</param>
        /// <returns>一个任务。</returns>
        public Task Subscribe(Url url, NotifyListenerDelegate listener)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 取消订阅一个注册中心。
        /// </summary>
        /// <param name="url">注册中心url。</param>
        /// <param name="listener">监听器。</param>
        /// <returns>一个任务。</returns>
        public Task UnSubscribe(Url url, NotifyListenerDelegate listener)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 发现注册中心中的所有服务。
        /// </summary>
        /// <param name="url">注册执行Url。</param>
        /// <returns>服务集合。</returns>
        public async Task<Url[]> Discover(Url url)
        {
            var parentPath = GetNodeTypePath(url, ZkNodeType.AvailableServer);
            var currentChilds = new List<string>();
            if (await _zooKeeper.existsAsync(parentPath) != null)
            {
                currentChilds = (await _zooKeeper.getChildrenAsync(parentPath)).Children;
            }
            return await GerUrls(parentPath, currentChilds);
        }

        #endregion Implementation of IDiscoveryService

        #region Private Method

        private async Task<Url[]> GerUrls(string parentPath, IEnumerable<string> currentChilds)
        {
            var urls = new List<Url>();

            if (currentChilds != null)
            {
                foreach (var node in currentChilds)
                {
                    var nodePath = $"{parentPath}/{node}";
                    var result = await _zooKeeper.getDataAsync(nodePath);
                    var data = Encoding.UTF8.GetString(result.Data);
                    urls.Add(new Url(data));
                }
            }

            return urls.ToArray();
        }

        private async Task CreateNode(Url url, ZkNodeType type)
        {
            var nodeTypePath = GetNodeTypePath(url, type);
            if (await _zooKeeper.existsAsync(nodeTypePath) == null)
            {
                await CreateSubdirectory(nodeTypePath, null, CreateMode.PERSISTENT);
            }
            await CreateSubdirectory(GetNodePath(url, type), Encoding.UTF8.GetBytes(url.ToString()), CreateMode.EPHEMERAL);
        }

        private async Task CreateSubdirectory(string path, byte[] data, CreateMode createMode)
        {
            if (await _zooKeeper.existsAsync(path) != null)
                return;

            var childrens = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var nodePath = "/";

            for (var i = 0; i < childrens.Length; i++)
            {
                var isLast = i == childrens.Length - 1;
                var children = childrens[i];
                nodePath += children;
                if (await _zooKeeper.existsAsync(nodePath) == null)
                {
                    await _zooKeeper.createAsync(nodePath, isLast ? data : null, ZooDefs.Ids.OPEN_ACL_UNSAFE, createMode);
                }
                nodePath += "/";
            }
        }

        private async Task RemoveNode(Url url, ZkNodeType type)
        {
            var nodeTypePath = GetNodePath(url, type);
            if (await _zooKeeper.existsAsync(nodeTypePath) != null)
            {
                await ZKUtil.deleteRecursiveAsync(_zooKeeper, nodeTypePath);
            }
        }

        private static string GetServicePath(Uri url)
        {
            var path = url.AbsolutePath;
            if (!path.StartsWith("/"))
                path = path.Insert(0, "/");
            return $"/rabbitcloud{path}";
        }

        private static string GetNodePath(Uri url, ZkNodeType nodeType)
        {
            return $"{GetNodeTypePath(url, nodeType)}/{url.Host}:{url.Port}";
        }

        private static string GetNodeTypePath(Uri url, ZkNodeType nodeType)
        {
            string type;
            switch (nodeType)
            {
                case ZkNodeType.AvailableServer:
                    type = "server";
                    break;

                case ZkNodeType.UnavailableServer:
                    type = "unavailableServer";
                    break;

                case ZkNodeType.Client:
                    type = "client";
                    break;

                default:
                    throw new NotSupportedException(nodeType.ToString());
            }
            return GetServicePath(url) + "/" + type;
        }

        #endregion Private Method
    }
}