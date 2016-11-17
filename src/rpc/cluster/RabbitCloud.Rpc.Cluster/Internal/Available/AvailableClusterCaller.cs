using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Cluster.Abstractions;
using RabbitCloud.Rpc.Cluster.Abstractions.Internal;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.Internal.Available
{
    public class AvailableClusterCaller : ClusterCaller
    {
        public AvailableClusterCaller(IDirectory directory, ILoadBalance loadBalance)
            : base(directory, loadBalance)
        {
        }

        #region Overrides of ClusterCaller

        /// <summary>
        /// 调用RPC请求。
        /// </summary>
        /// <param name="request">调用请求。</param>
        /// <param name="callers">调用者集合。</param>
        /// <param name="loadBalance">负载均衡器。</param>
        /// <returns>RPC请求响应结果。</returns>
        protected override async Task<IResponse> DoCall(IRequest request, IEnumerable<ICaller> callers, ILoadBalance loadBalance)
        {
            var caller = await Select(loadBalance, request, callers);
            return await caller.Call(request);
        }

        #endregion Overrides of ClusterCaller
    }
}