using RabbitCloud.Rpc.Abstractions;

namespace RabbitCloud.Rpc.Cluster.Abstractions
{
    /// <summary>
    /// 一个抽象的集群调用者。
    /// </summary>
    public interface ICluster
    {
        /// <summary>
        /// 加入指定的调用者目录。
        /// </summary>
        /// <param name="directory">调用者目录。</param>
        /// <returns>最终调用者。</returns>
        ICaller Join(IDirectory directory);
    }
}