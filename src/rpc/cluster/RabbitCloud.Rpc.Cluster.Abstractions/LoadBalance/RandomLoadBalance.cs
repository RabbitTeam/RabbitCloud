using RabbitCloud.Rpc.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.Abstractions.LoadBalance
{
    /// <summary>
    /// 随机规则的负载均衡实现。
    /// </summary>
    public class RandomLoadBalance : LoadBalance
    {
        private readonly Random _random = new Random();

        #region Overrides of LoadBalance

        /// <summary>
        /// 根据RPC请求信息选择一个RPC引用。
        /// </summary>
        /// <param name="request">RPC请求信息。</param>
        /// <returns>RPC引用。</returns>
        protected override Task<IReferer> DoSelect(IRequest request)
        {
            var referers = Referers;

            var idx = (int)(_random.NextDouble() * referers.Count);
            for (var i = 0; i < referers.Count; i++)
            {
                var referer = referers[(i + idx) % referers.Count];
                if (referer.IsAvailable)
                {
                    return Task.FromResult(referer);
                }
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

            var idx = (int)(_random.NextDouble() * referers.Count);
            for (var i = 0; i < referers.Count; i++)
            {
                var referer = referers.ElementAt((i + idx) % referers.Count);
                if (referer.IsAvailable)
                {
                    refersHolder.Add(referer);
                }
            }

            return Task.CompletedTask;
        }

        #endregion Overrides of LoadBalance
    }
}