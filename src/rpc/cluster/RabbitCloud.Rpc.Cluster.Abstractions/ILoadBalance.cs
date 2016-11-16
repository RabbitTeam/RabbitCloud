using RabbitCloud.Rpc.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.Abstractions
{
    /// <summary>
    /// 一个抽象的负载均衡接口。
    /// </summary>
    public interface ILoadBalance
    {
        /// <summary>
        /// 从调用者集合中选择一个用于调用的调用者。
        /// </summary>
        /// <param name="callers">调用者集合。</param>
        /// <param name="request">RPC请求。</param>
        /// <returns>调用者。</returns>
        Task<ICaller> Select(IEnumerable<ICaller> callers, IRequest request);
    }
}