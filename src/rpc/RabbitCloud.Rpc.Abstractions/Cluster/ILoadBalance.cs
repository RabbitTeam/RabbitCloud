using System.Collections.Generic;

namespace RabbitCloud.Rpc.Abstractions.Cluster
{
    public interface ILoadBalance
    {
        IEnumerable<ICaller> Callers { get; set; }

        ICaller Select(IRequest request);
    }
}