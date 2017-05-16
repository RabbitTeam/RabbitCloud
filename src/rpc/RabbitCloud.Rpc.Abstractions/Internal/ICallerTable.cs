using RabbitCloud.Abstractions;

namespace RabbitCloud.Rpc.Abstractions.Internal
{
    public interface ICallerTable
    {
        ICaller GetCaller(ServiceDescriptor descriptor);
    }
}