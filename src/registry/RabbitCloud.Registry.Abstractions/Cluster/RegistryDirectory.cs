using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Protocol;
using RabbitCloud.Rpc.Cluster.Directory;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitCloud.Registry.Abstractions.Cluster
{
    public class RegistryDirectory : Directory
    {
        private readonly IRegistry _registry;
        private readonly IProtocol _protocol;
        private IList<Url> _cacheUrls;

        public RegistryDirectory(IRegistry registry, IProtocol protocol, Type serviceType, Url url) : base(url)
        {
            _registry = registry;
            _protocol = protocol;
            InterfaceType = serviceType;

            _registry.Subscribe(Url, NotifyListenerDelegate).Wait();
        }

        #region Overrides of Directory

        /// <summary>
        /// 执行与释放或重置非托管资源关联的应用程序定义的任务。
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            _registry.UnSubscribe(Url, NotifyListenerDelegate).Wait();
        }

        #endregion Overrides of Directory

        #region Overrides of Directory

        /// <summary>
        /// 是否可用。
        /// </summary>
        public override bool IsAvailable { get; } = true;

        /// <summary>
        /// 服务接口类型。
        /// </summary>
        public override Type InterfaceType { get; }

        /// <summary>
        /// 根据RPC请求获取该服务所有的调用者。
        /// </summary>
        /// <param name="request">RPC请求。</param>
        /// <returns>调用者集合。</returns>
        protected override async Task<IEnumerable<ICaller>> DoGetCallers(IRequest request)
        {
            var urls = await GetUrls();
            var list = new List<ICaller>();

            foreach (var url in urls)
            {
                var referer = await _protocol.Refer(InterfaceType, url);
                list.Add(referer);
            }
            return list.ToArray();
        }

        #endregion Overrides of Directory

        #region Private Method

        private Task NotifyListenerDelegate(Url registryUrl, Url[] urls)
        {
            lock (this)
                _cacheUrls = new List<Url>(urls);
            return Task.CompletedTask;
        }

        private async Task<IEnumerable<Url>> GetUrls()
        {
            if (_cacheUrls != null)
                return _cacheUrls;

            Monitor.Enter(this);
            try
            {
                _cacheUrls = await _registry.Discover(Url);
            }
            finally
            {
                Monitor.Exit(this);
            }

            return _cacheUrls;
        }

        #endregion Private Method
    }
}