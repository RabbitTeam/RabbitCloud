using RabbitCloud.Rpc.Abstractions.Cluster;

namespace RabbitCloud.Config.Abstractions.Support
{
    public interface IClusterFactory
    {
        ICluster CreateCluster(string clusterName, string loadBalanceName, string haStrategyName);
    }
}