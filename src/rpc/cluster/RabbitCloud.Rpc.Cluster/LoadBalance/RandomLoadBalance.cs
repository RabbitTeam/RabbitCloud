using RabbitCloud.Rpc.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.LoadBalance
{
    /// <summary>
    /// 随机规则的负载均衡实现。
    /// </summary>
    public class RandomLoadBalance : LoadBalance
    {
        private readonly Random _random = new Random();

        #region Overrides of LoadBalance

        /// <summary>
        /// 从调用者集合中选择一个用于调用的调用者。
        /// </summary>
        /// <param name="callers">调用者集合。</param>
        /// <param name="request">RPC请求。</param>
        /// <returns>调用者。</returns>
        protected override Task<ICaller> DoSelect(IEnumerable<ICaller> callers, IRequest request)
        {
            var referers = callers.ToArray();

            var idx = (int)(_random.NextDouble() * referers.Length);
            for (var i = 0; i < referers.Length; i++)
            {
                var referer = referers[(i + idx) % referers.Length];
                if (referer.IsAvailable)
                {
                    return Task.FromResult(referer);
                }
            }

            return Task.FromResult<ICaller>(null);
        }

        #endregion Overrides of LoadBalance
    }
}