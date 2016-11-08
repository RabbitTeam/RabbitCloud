using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;

namespace RabbitCloud.Rpc.Cluster
{
    /// <summary>
    /// 一个抽象的调用者路由。
    /// </summary>
    public interface IRouter
    {
        /// <summary>
        /// 节点Url。
        /// </summary>
        Url Url { get; }

        /// <summary>
        /// 过滤调用者集合。
        /// </summary>
        /// <param name="invokers">调用者集合。</param>
        /// <param name="url">节点Url。</param>
        /// <param name="invocation">调用信息。</param>
        /// <returns>过滤之后的调用者集合。</returns>
        IInvoker[] Route(IInvoker[] invokers, Url url, IInvocation invocation);
    }
}