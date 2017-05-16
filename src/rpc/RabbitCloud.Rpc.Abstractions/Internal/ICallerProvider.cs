using RabbitCloud.Abstractions;
using System.Collections.Generic;

namespace RabbitCloud.Rpc.Abstractions.Internal
{
    public interface ICallerProvider
    {
        void GetCallers(IDictionary<ServiceDescriptor, ICaller> callers);
    }
}