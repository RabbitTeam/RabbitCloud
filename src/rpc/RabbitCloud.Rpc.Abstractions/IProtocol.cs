using RabbitCloud.Abstractions;

namespace RabbitCloud.Rpc.Abstractions
{
    public interface IProtocol
    {
        IExporter Export(IInvoker invoker);

        IInvoker Refer(Url url);
    }
}