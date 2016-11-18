using Newtonsoft.Json;
using RabbitCloud.Abstractions;
using RabbitCloud.Registry.Abstractions;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitCloud.Registry.Redis
{
    public class RedisRegistry : IRegistry, IDisposable
    {
        #region Field

        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly IDatabase _database;
        private readonly ISubscriber _subscriber;

        private readonly string _applicationId;
        private readonly string _subscriberChannel;

        private readonly ConcurrentDictionary<string, Lazy<Task<List<Url>>>> _registeredServiceUrls = new ConcurrentDictionary<string, Lazy<Task<List<Url>>>>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, IList<NotifyListenerDelegate>> _notifyListeners =
            new ConcurrentDictionary<string, IList<NotifyListenerDelegate>>(StringComparer.OrdinalIgnoreCase);

        private readonly string _registryId = Guid.NewGuid().ToString("N");

        #endregion Field

        public RedisRegistry(RedisConnectionInfo connectionInfo)
        {
            _connectionMultiplexer = ConnectionMultiplexer.Connect(connectionInfo.ConnectionString);
            _database = _connectionMultiplexer.GetDatabase(connectionInfo.Database);
            _subscriber = _connectionMultiplexer.GetSubscriber();

            _applicationId = connectionInfo.ApplicationId;
            _subscriberChannel = $"{_applicationId}_Subscriber";

            _subscriber.Subscribe(_subscriberChannel, async (channel, value) =>
            {
                var message = JsonConvert.DeserializeObject<PublishMessage>(value);
                if (message.Id == _registryId)
                    return;
                var url = new Url(message.Url);
                var urls = await Discover(url);

                await NotifyListener(url, urls);
            });
        }

        #region Implementation of IRegistryService

        /// <summary>
        /// 注册一个服务。
        /// </summary>
        /// <param name="url">服务Url。</param>
        /// <returns>一个任务。</returns>
        public async Task Register(Url url)
        {
            await RemoveEntry(url, NodeType.AvailableServer);
            await RemoveEntry(url, NodeType.UnavailableServer);
            await CreateEntry(url, NodeType.AvailableServer);

            //添加到本地缓存
            var list = await GetUrlList(url);
            list.Remove(url);
            list.Add(url);

            await PublishListener(url);
        }

        /// <summary>
        /// 取消注册一个服务。
        /// </summary>
        /// <param name="url">服务Url。</param>
        /// <returns>一个任务。</returns>
        public async Task UnRegister(Url url)
        {
            await RemoveEntry(url, NodeType.AvailableServer);
            await RemoveEntry(url, NodeType.UnavailableServer);

            //从本地缓存中删除
            var list = await GetUrlList(url);
            list.Remove(url);

            await PublishListener(url);
        }

        #endregion Implementation of IRegistryService

        #region Implementation of IDiscoveryService

        /// <summary>
        /// 订阅一个服务。
        /// </summary>
        /// <param name="url">服务Url。</param>
        /// <param name="listener">监听器。</param>
        /// <returns>一个任务。</returns>
        public Task Subscribe(Url url, NotifyListenerDelegate listener)
        {
            var key = GetServicePath(url);

            var listeners = _notifyListeners.GetOrAdd(key, new List<NotifyListenerDelegate>());
            if (!listeners.Contains(listener))
                listeners.Add(listener);

            return Task.CompletedTask;
        }

        /// <summary>
        /// 取消订阅。
        /// </summary>
        /// <param name="url">服务Url。</param>
        /// <param name="listener">监听器。</param>
        /// <returns>一个任务。</returns>
        public Task UnSubscribe(Url url, NotifyListenerDelegate listener)
        {
            var key = GetServicePath(url);

            IList<NotifyListenerDelegate> listeners;
            if (!_notifyListeners.TryGetValue(key, out listeners))
                return Task.CompletedTask;

            listeners.Remove(listener);

            return Task.CompletedTask;
        }

        private async Task<List<Url>> GetUrlList(Url url)
        {
            var key = GetServicePath(url);
            return await _registeredServiceUrls.GetOrAdd(key, new Lazy<Task<List<Url>>>(async () =>
            {
                var urls = await GetUrls(url, NodeType.AvailableServer);
                var list = new List<Url>();
                foreach (var urlString in urls)
                {
                    list.Add(new Url(urlString));
                }
                return list;
            })).Value;
        }

        /// <summary>
        /// 发现注册中心中指定服务的所有节点。
        /// </summary>
        /// <param name="url">注册的服务Url。</param>
        /// <returns>服务节点集合。</returns>
        public async Task<Url[]> Discover(Url url)
        {
            var list = await GetUrlList(url);
            return list.ToArray();
        }

        #endregion Implementation of IDiscoveryService

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            _connectionMultiplexer.Dispose();
        }

        #endregion Implementation of IDisposable

        #region Private Method

        internal struct PublishMessage
        {
            public PublishMessage(string id, Url url)
            {
                Id = id;
                Url = url.ToString();
            }

            public string Id { get; set; }
            public string Url { get; set; }
        }

        /// <summary>
        /// 发布一个服务的监听事件。
        /// </summary>
        private async Task PublishListener(Url url)
        {
            await NotifyListener(url, await Discover(url));
            _subscriber.Publish(_subscriberChannel, JsonConvert.SerializeObject(new PublishMessage(_registryId, url)));
        }

        private async Task NotifyListener(Url registryUrl, Url[] urls)
        {
            var key = GetNodeTypePath(registryUrl, NodeType.AvailableServer);
            IList<NotifyListenerDelegate> listeners;
            if (!_notifyListeners.TryGetValue(key, out listeners))
                return;

            foreach (var listener in listeners)
            {
                await listener(registryUrl, urls);
            }
        }

        /// <summary>
        /// 获取节点路径（/user/account/server/127.0.0.1:9981）
        /// </summary>
        private static string GetNodePath(Url url, NodeType nodeType)
        {
            return $"{GetNodeTypePath(url, nodeType)}/{url.Host}:{url.Port}";
        }

        /// <summary>
        /// 获取节点类型路径（/user/account/server）
        /// </summary>
        private static string GetNodeTypePath(Url url, NodeType nodeType)
        {
            string type;
            switch (nodeType)
            {
                case NodeType.AvailableServer:
                    type = "server";
                    break;

                case NodeType.UnavailableServer:
                    type = "unavailableServer";
                    break;

                case NodeType.Client:
                    type = "client";
                    break;

                default:
                    throw new NotSupportedException(nodeType.ToString());
            }
            return $"{GetServicePath(url)}/{type}";
        }

        /// <summary>
        /// 获取服务路径（例：rabbitcloud://127.0.0.1:9981/user/account?fast=true => /user/account）
        /// </summary>
        private static string GetServicePath(Url url)
        {
            var path = url.Path;
            if (!path.StartsWith("/"))
                path = path.Insert(0, "/");
            return path;
        }

        private async Task RemoveEntry(Url url, NodeType nodeType)
        {
            var hashKey = _applicationId;
            var nodeTypePath = GetNodeTypePath(url, nodeType);
            var nodePah = GetNodePath(url, nodeType);

            //得到当前
            var currentUrls = await GetUrls(url, nodeType);

            currentUrls = currentUrls.Where(i => GetNodePath(new Url(i), nodeType) != nodePah).ToArray();

            _database.HashSet(hashKey, nodeTypePath, JsonConvert.SerializeObject(currentUrls));
        }

        private Task<string[]> GetUrls(Url url, NodeType nodeType)
        {
            var hashKey = _applicationId;
            var nodeTypePath = GetNodeTypePath(url, nodeType);

            //todo: HashGetAsync 会无限等待
            var currentUrlContent = _database.HashGet(hashKey, nodeTypePath);

            //得到当前
            var currentUrls = currentUrlContent.HasValue ? JsonConvert.DeserializeObject<string[]>(currentUrlContent) : new string[0];
            return Task.FromResult(currentUrls);
        }

        private async Task CreateEntry(Url url, NodeType nodeType)
        {
            var hashKey = _applicationId;
            var nodeTypePath = GetNodeTypePath(url, nodeType);

            //得到当前
            var currentUrls = await GetUrls(url, nodeType);

            var createUrl = url.ToString();
            if (currentUrls.Contains(createUrl))
                return;

            _database.HashSet(hashKey, nodeTypePath, JsonConvert.SerializeObject(currentUrls.Concat(new[] { createUrl })));
        }

        #endregion Private Method
    }
}