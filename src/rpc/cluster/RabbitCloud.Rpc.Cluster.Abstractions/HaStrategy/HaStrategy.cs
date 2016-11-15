using RabbitCloud.Rpc.Abstractions;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.Abstractions.HaStrategy
{
    public abstract class HaStrategy : IHaStrategy
    {
        #region Implementation of IHaStrategy

        /// <summary>
        /// 进行调用。
        /// </summary>
        /// <param name="request">RPC请求。</param>
        /// <param name="loadBalance">负载均衡器。</param>
        /// <returns>RPC响应。</returns>
        public abstract Task<IResponse> Call(IRequest request, ILoadBalance loadBalance);

        #endregion Implementation of IHaStrategy
    }
}