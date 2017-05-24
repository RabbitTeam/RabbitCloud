using RabbitCloud.Rpc.Abstractions.Cluster;

namespace RabbitCloud.Config
{
    public interface IClusterProvider
    {
        string Name { get; }

        ICluster CreateCluster();
    }
}