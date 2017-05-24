using RabbitCloud.Rpc.Abstractions;

namespace RabbitCloud.Config.Abstractions.Support
{
    public interface IProtocolFactory
    {
        IProtocol GetProtocol(ProtocolConfig config);
    }
}