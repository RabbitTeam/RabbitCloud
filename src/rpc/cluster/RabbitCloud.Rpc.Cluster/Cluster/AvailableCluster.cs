using RabbitCloud.Rpc.Abstractions;

namespace RabbitCloud.Rpc.Cluster.Cluster
{
    public class AvailableCluster : ICluster
    {
        #region Implementation of ICluster

        /// <summary>
        /// 加入一个目录并返回改目录的集群策略调用者。
        /// </summary>
        /// <param name="directory">调用者目录。</param>
        /// <returns>集群策略调用者。</returns>
        public IInvoker Join(IDirectory directory)
        {
            return new AvailableClusterInvoker(directory);
        }

        #endregion Implementation of ICluster
    }
}