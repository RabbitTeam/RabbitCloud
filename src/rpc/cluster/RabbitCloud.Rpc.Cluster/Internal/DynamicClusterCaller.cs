using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Cluster.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.Internal
{
    public class DynamicClusterCaller : ClusterCaller
    {
        private readonly Func<DynamicClusterCaller, IRequest, IEnumerable<ICaller>, ILoadBalance, Task<IResponse>> _doCall;

        public DynamicClusterCaller(IDirectory directory, Func<DynamicClusterCaller, IRequest, IEnumerable<ICaller>, ILoadBalance, Task<IResponse>> doCall) : base(directory)
        {
            _doCall = doCall;
        }

        #region Overrides of ClusterCaller

        /// <summary>
        /// 调用RPC请求。
        /// </summary>
        /// <param name="request">调用请求。</param>
        /// <param name="callers">调用者集合。</param>
        /// <param name="loadBalance">负载均衡器。</param>
        /// <returns>RPC请求响应结果。</returns>
        protected override Task<IResponse> DoCall(IRequest request, IEnumerable<ICaller> callers, ILoadBalance loadBalance)
        {
            return _doCall(this, request, callers, loadBalance);
        }

        #endregion Overrides of ClusterCaller
    }
}