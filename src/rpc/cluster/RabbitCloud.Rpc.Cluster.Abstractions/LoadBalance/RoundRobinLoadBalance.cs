using RabbitCloud.Rpc.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.Abstractions.LoadBalance
{
    /// <summary>
    /// 轮询规则的负载均衡实现。
    /// </summary>
    public class RoundRobinLoadBalance : LoadBalance
    {
        private int _identity = -1;

        #region Overrides of LoadBalance

        /// <summary>
        /// 根据RPC请求信息选择一个RPC引用。
        /// </summary>
        /// <param name="request">RPC请求信息。</param>
        /// <returns>RPC引用。</returns>
        protected override Task<IReferer> DoSelect(IRequest request)
        {
            var referers = Referers;
            var index = GetNextPositive();
            for (var i = 0; i < referers.Count; i++)
            {
                var referer = referers[(i + index) % referers.Count];
                if (referer.IsAvailable)
                    return Task.FromResult(referer);
            }
            return Task.FromResult<IReferer>(null);
        }

        /// <summary>
        /// 根据RPC请求信息选择一组服务引用。
        /// </summary>
        /// <param name="request">RPC请求。</param>
        /// <param name="refersHolder">服务引用持有者。</param>
        /// <returns>一个任务。</returns>
        protected override Task DoSelectToHolder(IRequest request, IList<IReferer> refersHolder)
        {
            var referers = Referers;
            var index = GetNextPositive();
            for (var i = 0; i < referers.Count; i++)
            {
                var referer = referers[(i + index) % referers.Count];
                if (referer.IsAvailable)
                    refersHolder.Add(referer);
            }

            return Task.CompletedTask;
        }

        #endregion Overrides of LoadBalance

        private int GetNextPositive()
        {
            return Interlocked.Increment(ref _identity);
        }
    }
}