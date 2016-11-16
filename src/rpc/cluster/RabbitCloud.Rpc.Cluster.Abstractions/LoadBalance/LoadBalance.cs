using RabbitCloud.Rpc.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.Abstractions.LoadBalance
{
    /// <summary>
    /// 负载均衡抽象类。
    /// </summary>
    public abstract class LoadBalance : ILoadBalance
    {
        #region Implementation of ILoadBalance

        /// <summary>
        /// 从调用者集合中选择一个用于调用的调用者。
        /// </summary>
        /// <param name="callers">调用者集合。</param>
        /// <param name="request">RPC请求。</param>
        /// <returns>调用者。</returns>
        public async Task<ICaller> Select(IEnumerable<ICaller> callers, IRequest request)
        {
            if (callers == null)
                return null;
            callers = callers.ToArray();
            if (callers.Count() <= 1)
                return callers.FirstOrDefault();

            return await DoSelect(callers, request);
        }

        #endregion Implementation of ILoadBalance

        #region Protected  Method

        /// <summary>
        /// 从调用者集合中选择一个用于调用的调用者。
        /// </summary>
        /// <param name="callers">调用者集合。</param>
        /// <param name="request">RPC请求。</param>
        /// <returns>调用者。</returns>
        protected abstract Task<ICaller> DoSelect(IEnumerable<ICaller> callers, IRequest request);

        #endregion Protected  Method
    }
}