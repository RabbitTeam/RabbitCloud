using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using System;

namespace RabbitCloud.Rpc.Cluster.LoadBalance
{
    public class RandomLoadBalance : LoadBalanceBase
    {
        private readonly Random _random = new Random();

        #region Overrides of LoadBalanceBase

        /// <summary>
        /// 从调用者集合中选择一个合适的调用者。
        /// </summary>
        /// <param name="invokers">调用者集合。</param>
        /// <param name="url">节点Url。</param>
        /// <param name="invocation">调用信息。</param>
        /// <returns>最终调用者。</returns>
        protected override IInvoker DoSelect(IInvoker[] invokers, Url url, IInvocation invocation)
        {
            return invokers[_random.Next(0, invokers.Length)];
        }

        #endregion Overrides of LoadBalanceBase
    }
}