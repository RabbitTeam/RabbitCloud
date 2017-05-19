using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Cluster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.HA
{
    public class FailoverHaStrategy : IHaStrategy
    {
        private readonly IEnumerable<ICaller> _callers;

        public FailoverHaStrategy(IEnumerable<ICaller> callers)
        {
            _callers = callers;
        }

        #region Implementation of IHaStrategy

        public async Task<IResponse> CallAsync(IRequest request, ILoadBalance loadBalance)
        {
            var count = _callers.Count();
            Exception exception = null;
            for (var i = 0; i < count; i++)
            {
                var caller = loadBalance.Select(_callers, request);
                try
                {
                    return await caller.CallAsync(request);
                }
                catch (Exception e)
                {
                    exception = e;
                }
            }
            if (exception != null)
                throw exception;

            return null;
        }

        #endregion Implementation of IHaStrategy
    }
}