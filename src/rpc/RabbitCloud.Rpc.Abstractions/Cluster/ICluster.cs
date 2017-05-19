using System;

namespace RabbitCloud.Rpc.Abstractions.Cluster
{
    public interface ICluster : ICaller, IDisposable
    {
        ILoadBalance LoadBalance { get; set; }
        IHaStrategy HaStrategy { get; set; }
    }
}