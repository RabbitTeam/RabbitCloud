using RabbitCloud.Rpc.Abstractions.Cluster;

namespace RabbitCloud.Config
{
    public interface ILoadBalanceProvider
    {
        string Name { get; }

        ILoadBalance CreateLoadBalance();
    }
}