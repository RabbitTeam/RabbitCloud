using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Cluster;
using System.Collections.Generic;
using System.Linq;

namespace RabbitCloud.Rpc.Cluster.LoadBalance
{
    public abstract class LoadBalanceBase : ILoadBalance
    {
        #region Implementation of ILoadBalance

        public ICaller Select(IEnumerable<ICaller> callers, IRequest request)
        {
            if (callers == null)
                return null;
            var callerArray = callers.ToArray();
            return callerArray.Length == 1 ? callerArray[0] : DoSelect(callerArray, request);
        }

        #endregion Implementation of ILoadBalance

        protected abstract ICaller DoSelect(ICaller[] callers, IRequest request);
    }
}