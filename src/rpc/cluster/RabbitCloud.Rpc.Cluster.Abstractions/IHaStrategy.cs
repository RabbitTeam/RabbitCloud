using RabbitCloud.Rpc.Abstractions;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.Abstractions
{
    /// <summary>
    /// 一个抽象的高可用策略。
    /// </summary>
    public interface IHaStrategy
    {
        /// <summary>
        /// 进行调用。
        /// </summary>
        /// <param name="request">RPC请求。</param>
        /// <param name="loadBalance">负载均衡器。</param>
        /// <returns>RPC响应。</returns>
        Task<IResponse> Call(IRequest request, ILoadBalance loadBalance);
    }
}