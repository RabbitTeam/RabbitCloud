using RabbitCloud.Abstractions.Exceptions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Cluster;
using System;
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
                throw new ArgumentNullException(nameof(callers));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var callerArray = callers.ToArray();

            var caller = callerArray.Length > 1 ? DoSelect(callerArray, request) : callerArray.FirstOrDefault(i => i.IsAvailable);

            if (caller == null)
                throw new RabbitServiceException($"{GetType().Name} No available referers for call request:{request}");
            return caller;
        }

        #endregion Implementation of ILoadBalance

        protected abstract ICaller DoSelect(ICaller[] callers, IRequest request);
    }
}