using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Cluster;
using System.Collections.Generic;

namespace RabbitCloud.Config
{
    public interface IClusterProvider
    {
        string Name { get; }

        ICluster CreateCluster(IEnumerable<ICaller> callers, ILoadBalance loadBalance, IHaStrategy haStrategy);
    }
}