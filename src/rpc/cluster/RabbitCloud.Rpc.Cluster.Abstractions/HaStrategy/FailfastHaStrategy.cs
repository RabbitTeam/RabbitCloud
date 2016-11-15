using RabbitCloud.Rpc.Abstractions;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.Abstractions.HaStrategy
{
    /// <summary>
    /// 快速失败高可用策略。
    /// </summary>
    public class FailfastHaStrategy : HaStrategy
    {
        #region Overrides of HaStrategy

        /// <summary>
        /// 进行调用。
        /// </summary>
        /// <param name="request">RPC请求。</param>
        /// <param name="loadBalance">负载均衡器。</param>
        /// <returns>RPC响应。</returns>
        public override async Task<IResponse> Call(IRequest request, ILoadBalance loadBalance)
        {
            var referer = await loadBalance.Select(request);
            return await referer.Call(request);
        }

        #endregion Overrides of HaStrategy
    }
}