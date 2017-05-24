using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Cluster;
using System.Collections.Generic;

namespace RabbitCloud.Config.Abstractions.Support
{
    public interface IClusterFactory
    {
        ICluster CreateCluster(IEnumerable<ICaller> callers, string clusterName, string loadBalanceName, string haStrategyName);
    }
}