using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Cluster;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.HA
{
    public class FailfastHaStrategy : IHaStrategy
    {
        #region Implementation of IHaStrategy

        public Task<IResponse> CallAsync(IRequest request, ILoadBalance loadBalance)
        {
            var caller = loadBalance.Select(request);
            return caller.CallAsync(request);
        }

        #endregion Implementation of IHaStrategy
    }
}