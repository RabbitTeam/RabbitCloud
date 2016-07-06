using Rabbit.Rpc.Messages;

namespace Rabbit.Rpc.Transport.Codec
{
    public interface ITransportMessageEncoder
    {
        byte[] Encode(TransportMessage message);
    }
}