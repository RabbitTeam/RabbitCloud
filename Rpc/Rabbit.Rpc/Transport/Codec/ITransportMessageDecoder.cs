using Rabbit.Rpc.Messages;

namespace Rabbit.Rpc.Transport.Codec
{
    public interface ITransportMessageDecoder
    {
        TransportMessage Decode(byte[] data);
    }
}