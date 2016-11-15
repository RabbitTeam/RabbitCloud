using RabbitCloud.Rpc.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.Abstractions.LoadBalance
{
    public abstract class LoadBalance : ILoadBalance
    {
        protected IList<IReferer> Referers { get; private set; }

        #region Implementation of ILoadBalance

        /// <summary>
        /// 刷新服务引用。
        /// </summary>
        /// <param name="referers">服务引用集合。</param>
        public void OnRefresh(IEnumerable<IReferer> referers)
        {
            Referers = new List<IReferer>(referers);
        }

        /// <summary>
        /// 根据RPC请求信息选择一个RPC引用。
        /// </summary>
        /// <param name="request">RPC请求信息。</param>
        /// <returns>RPC引用。</returns>
        public async Task<IReferer> Select(IRequest request)
        {
            var referers = Referers;

            IReferer referer = null;
            if (referers.Count > 1)
            {
                referer = await DoSelect(request);
            }
            else if (referers.Count == 1)
            {
                referer = referers.First();
                if (!referer.IsAvailable)
                    referer = null;
            }

            if (referer != null)
            {
                return referer;
            }
            throw new Exception("No available referers for call");
        }

        /// <summary>
        /// 根据RPC请求信息选择一组服务引用。
        /// </summary>
        /// <param name="request">RPC请求。</param>
        /// <param name="refersHolder">服务引用持有者。</param>
        /// <returns>一个任务。</returns>
        public async Task SelectToHolder(IRequest request, IList<IReferer> refersHolder)
        {
            var referers = Referers;

            if (referers.Count > 1)
            {
                await DoSelectToHolder(request, refersHolder);
            }
            else if (referers.Count == 1 && referers.First().IsAvailable)
            {
                refersHolder.Add(referers.First());
            }
            if (!refersHolder.Any())
                throw new Exception("No available referers for call");
        }

        #endregion Implementation of ILoadBalance

        #region Public Method

        /// <summary>
        /// 根据RPC请求信息选择一个RPC引用。
        /// </summary>
        /// <param name="request">RPC请求信息。</param>
        /// <returns>RPC引用。</returns>
        protected abstract Task<IReferer> DoSelect(IRequest request);

        /// <summary>
        /// 根据RPC请求信息选择一组服务引用。
        /// </summary>
        /// <param name="request">RPC请求。</param>
        /// <param name="refersHolder">服务引用持有者。</param>
        /// <returns>一个任务。</returns>
        protected abstract Task DoSelectToHolder(IRequest request, IList<IReferer> refersHolder);

        #endregion Public Method
    }
}