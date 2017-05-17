using System.Collections.Generic;

namespace RabbitCloud.Rpc.Abstractions
{
    public interface IGroupCaller : ICaller
    {
        IEnumerable<INamedCaller> Callers { get; }
    }
}