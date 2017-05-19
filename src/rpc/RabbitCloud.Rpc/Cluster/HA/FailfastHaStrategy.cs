using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Cluster;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.HA
{
    public class FailfastHaStrategy : IHaStrategy
    {
        private readonly IEnumerable<ICaller> _callers;

        public FailfastHaStrategy(IEnumerable<ICaller> callers)
        {
            _callers = callers;
        }

        #region Implementation of IHaStrategy

        public Task<IResponse> CallAsync(IRequest request, ILoadBalance loadBalance)
        {
            var caller = loadBalance.Select(_callers, request);
            return caller.CallAsync(request);
        }

        #endregion Implementation of IHaStrategy
    }
}