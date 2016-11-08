using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using System.Linq;

namespace RabbitCloud.Rpc.Cluster.LoadBalance
{
    public abstract class LoadBalanceBase : ILoadBalance
    {
        #region Implementation of ILoadBalance

        /// <summary>
        /// 从调用者集合中选择一个合适的调用者。
        /// </summary>
        /// <param name="invokers">调用者集合。</param>
        /// <param name="url">节点Url。</param>
        /// <param name="invocation">调用信息。</param>
        /// <returns>最终调用者。</returns>
        public IInvoker Select(IInvoker[] invokers, Url url, IInvocation invocation)
        {
            if (invokers == null || !invokers.Any())
            {
                return null;
            }
            return invokers.Length == 1 ? invokers[0] : DoSelect(invokers, url, invocation);
        }

        #endregion Implementation of ILoadBalance

        #region Protected Method

        /// <summary>
        /// 从调用者集合中选择一个合适的调用者。
        /// </summary>
        /// <param name="invokers">调用者集合。</param>
        /// <param name="url">节点Url。</param>
        /// <param name="invocation">调用信息。</param>
        /// <returns>最终调用者。</returns>
        protected abstract IInvoker DoSelect(IInvoker[] invokers, Url url, IInvocation invocation);

        #endregion Protected Method
    }
}