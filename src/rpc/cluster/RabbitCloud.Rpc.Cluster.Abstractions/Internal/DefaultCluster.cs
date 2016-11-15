using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.Abstractions.Internal
{
    public class DefaultCluster : ICluster
    {
        private IReferer[] _referers;
        private volatile bool _available = true;

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            _available = false;
            foreach (var referer in _referers)
            {
                referer.Dispose();
            }
        }

        #endregion Implementation of IDisposable

        #region Implementation of INode

        /// <summary>
        /// 节点Url。
        /// </summary>
        public Url Url { get; private set; }

        /// <summary>
        /// 是否可用。
        /// </summary>
        public bool IsAvailable
        {
            get { return _available; }
            set { _available = value; }
        }

        #endregion Implementation of INode

        #region Implementation of ICaller

        /// <summary>
        /// 接口类型。
        /// </summary>
        public Type InterfaceType => _referers.First().InterfaceType;

        /// <summary>
        /// 调用RPC请求。
        /// </summary>
        /// <param name="request">调用请求。</param>
        /// <returns>RPC请求响应结果。</returns>
        public async Task<IResponse> Call(IRequest request)
        {
            if (!_available)
                throw new RpcException("无法完成调用");
            return await HaStrategy.Call(request, LoadBalance);
        }

        #endregion Implementation of ICaller

        #region Implementation of ICluster

        public void SetUrl(Url url)
        {
            Url = url;
        }

        /// <summary>
        /// 集群所使用的负载均衡器。
        /// </summary>
        public ILoadBalance LoadBalance { get; set; }

        /// <summary>
        /// 集群所使用的高可用策略。
        /// </summary>
        public IHaStrategy HaStrategy { private get; set; }

        /// <summary>
        /// 刷新服务引用。
        /// </summary>
        /// <param name="referers">服务引用集合。</param>
        public void OnRefresh(IEnumerable<IReferer> referers)
        {
            if (referers == null)
                return;
            referers = referers.ToArray();

            if (!referers.Any())
                return;

            LoadBalance.OnRefresh(referers);
            var oldReferers = _referers;
            _referers = (IReferer[])referers;

            if (oldReferers == null || !oldReferers.Any())
                return;

            foreach (var referer in oldReferers)
            {
                if (referers.Contains(referer))
                    continue;
                referer.Dispose();
            }
        }

        /// <summary>
        /// 获取集群中所有的服务引用。
        /// </summary>
        /// <returns>服务引用集合。</returns>
        public IEnumerable<IReferer> GetReferers()
        {
            return _referers;
        }

        #endregion Implementation of ICluster
    }
}