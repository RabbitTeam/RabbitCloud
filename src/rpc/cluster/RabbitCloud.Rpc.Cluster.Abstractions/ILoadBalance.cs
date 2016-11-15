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
        /// 刷新服务引用。
        /// </summary>
        /// <param name="referers">服务引用集合。</param>
        void OnRefresh(IEnumerable<IReferer> referers);

        /// <summary>
        /// 根据RPC请求信息选择一个RPC引用。
        /// </summary>
        /// <param name="request">RPC请求信息。</param>
        /// <returns>RPC引用。</returns>
        Task<IReferer> Select(IRequest request);

        /// <summary>
        /// 根据RPC请求信息选择一组服务引用。
        /// </summary>
        /// <param name="request">RPC请求。</param>
        /// <param name="refersHolder">服务引用持有者。</param>
        /// <returns>一个任务。</returns>
        Task SelectToHolder(IRequest request, IList<IReferer> refersHolder);
    }
}