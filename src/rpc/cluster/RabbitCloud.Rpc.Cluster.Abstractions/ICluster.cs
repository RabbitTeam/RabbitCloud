using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using System.Collections.Generic;

namespace RabbitCloud.Rpc.Cluster.Abstractions
{
    /// <summary>
    /// 一个抽象的集群调用者。
    /// </summary>
    public interface ICluster : ICaller
    {
        void SetUrl(Url url);

        /// <summary>
        /// 集群所使用的负载均衡器。
        /// </summary>
        ILoadBalance LoadBalance { get; set; }

        /// <summary>
        /// 集群所使用的高可用策略。
        /// </summary>
        IHaStrategy HaStrategy { set; }

        /// <summary>
        /// 刷新服务引用。
        /// </summary>
        /// <param name="referers">服务引用集合。</param>
        void OnRefresh(IEnumerable<IReferer> referers);

        /// <summary>
        /// 获取集群中所有的服务引用。
        /// </summary>
        /// <returns>服务引用集合。</returns>
        IEnumerable<IReferer> GetReferers();
    }
}