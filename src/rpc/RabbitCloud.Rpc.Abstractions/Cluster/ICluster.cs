using System;

namespace RabbitCloud.Rpc.Abstractions.Cluster
{
    public interface ICluster : ICaller, IDisposable
    {
        ICaller[] Callers { get; set; }
        ILoadBalance LoadBalance { get; }
        IHaStrategy HaStrategy { get; }
    }
}