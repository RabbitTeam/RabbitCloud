using RabbitCloud.Rpc.Abstractions.Cluster;

namespace RabbitCloud.Config
{
    public interface IHaStrategyProvider
    {
        string Name { get; }

        IHaStrategy CreateHaStrategy();
    }
}