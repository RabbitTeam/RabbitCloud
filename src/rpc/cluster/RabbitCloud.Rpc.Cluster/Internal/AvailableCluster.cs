using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Exceptions;
using RabbitCloud.Rpc.Cluster.Abstractions;
using System.Linq;

namespace RabbitCloud.Rpc.Cluster.Internal
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
            return new DynamicClusterCaller(directory, async (cluster, request, callers, loadBalance) =>
            {
                var caller = callers.FirstOrDefault(i => i.IsAvailable);
                if (caller == null)
                    throw new RpcException("No provider available");
                return await caller.Call(request);
            });
        }

        #endregion Implementation of ICluster
    }
}