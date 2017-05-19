using System.Collections.Generic;

namespace RabbitCloud.Rpc.Abstractions.Cluster
{
    public interface ILoadBalance
    {
        ICaller Select(IEnumerable<ICaller> callers, IRequest request);
    }
}