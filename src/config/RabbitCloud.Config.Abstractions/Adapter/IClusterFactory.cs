using RabbitCloud.Rpc.Abstractions.Cluster;

namespace RabbitCloud.Config.Abstractions.Adapter
{
    public interface IClusterFactory
    {
        ICluster CreateCluster(string clusterName, string loadBalanceName, string haStrategyName);
    }

    public interface IClusterProvider
    {
        string Name { get; }

        ICluster CreateCluster();
    }

    public interface ILoadBalanceProvider
    {
        string Name { get; }

        ILoadBalance CreateLoadBalance();
    }

    public interface IHaStrategyProvider
    {
        string Name { get; }

        IHaStrategy CreateHaStrategy();
    }
}