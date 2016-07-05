using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Serialization;

namespace Rabbit.Rpc.Transport.Codec.Implementation
{
    public sealed class ByteTransportMessageEncoder : ITransportMessageEncoder
    {
        private readonly ISerializer<byte[]> _serializer;

        public ByteTransportMessageEncoder(ISerializer<byte[]> serializer)
        {
            _serializer = serializer;
        }

        #region Implementation of ITransportMessageEncoder

        public byte[] Encode(TransportMessage message)
        {
            return _serializer.Serialize(message);
        }

        #endregion Implementation of ITransportMessageEncoder
    }
}