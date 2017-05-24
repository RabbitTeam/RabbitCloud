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

        public IEnumerable<ICaller> Callers { get; set; }

        public ICaller Select(IRequest request)
        {
            if (Callers == null)
                throw new ArgumentNullException(nameof(Callers));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var callerArray = Callers.ToArray();

            var caller = callerArray.Length > 1 ? DoSelect(callerArray, request) : callerArray.FirstOrDefault(i => i.IsAvailable);

            if (caller == null)
                throw new RabbitServiceException($"{GetType().Name} No available referers for call request:{request}");
            return caller;
        }

        #endregion Implementation of ILoadBalance

        protected abstract ICaller DoSelect(ICaller[] callers, IRequest request);
    }
}