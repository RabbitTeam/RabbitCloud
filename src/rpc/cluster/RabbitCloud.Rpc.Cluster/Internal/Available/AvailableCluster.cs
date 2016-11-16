using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Cluster.Abstractions;

namespace RabbitCloud.Rpc.Cluster.Internal.Available
{
    /// <summary>
    /// 可用策略的集群实现。
    /// </summary>
    public class AvailableCluster : ICluster
    {
        #region Implementation of ICluster

        /// <summary>
        /// 加入指定的调用者目录。
        /// </summary>
        /// <param name="directory">调用者目录。</param>
        /// <returns>最终调用者。</returns>
        public ICaller Join(IDirectory directory)
        {
            return new AvailableClusterCaller(directory);
        }

        #endregion Implementation of ICluster
    }
}