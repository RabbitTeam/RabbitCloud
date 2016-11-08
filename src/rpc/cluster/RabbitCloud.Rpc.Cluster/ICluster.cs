using RabbitCloud.Rpc.Abstractions;

namespace RabbitCloud.Rpc.Cluster
{
    /// <summary>
    /// 一个抽象的集群。
    /// </summary>
    public interface ICluster
    {
        /// <summary>
        /// 加入一个目录并返回改目录的集群策略调用者。
        /// </summary>
        /// <param name="directory">调用者目录。</param>
        /// <returns>集群策略调用者。</returns>
        IInvoker Join(IDirectory directory);
    }
}