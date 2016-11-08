using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;

namespace RabbitCloud.Rpc.Cluster
{
    /// <summary>
    /// 一个抽象的负载均衡。
    /// </summary>
    public interface ILoadBalance
    {
        /// <summary>
        /// 从调用者集合中选择一个合适的调用者。
        /// </summary>
        /// <param name="invokers">调用者集合。</param>
        /// <param name="url">节点Url。</param>
        /// <param name="invocation">调用信息。</param>
        /// <returns>最终调用者。</returns>
        IInvoker Select(IInvoker[] invokers, Url url, IInvocation invocation);
    }
}