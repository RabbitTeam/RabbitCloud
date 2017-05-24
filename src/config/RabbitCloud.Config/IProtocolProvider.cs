using RabbitCloud.Rpc.Abstractions;

namespace RabbitCloud.Config.Abstractions.Adapter
{
    public interface IProtocolProvider
    {
        string Name { get; }

        IProtocol CreateProtocol(ProtocolConfig config);
    }
}