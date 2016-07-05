using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Serialization;

namespace Rabbit.Rpc.Transport.Codec.Implementation
{
    public sealed class ByteJsonTransportMessageDecoder : ITransportMessageDecoder
    {
        private readonly ISerializer<byte[]> _serializer;

        public ByteJsonTransportMessageDecoder(ISerializer<byte[]> serializer)
        {
            _serializer = serializer;
        }

        #region Implementation of ITransportMessageDecoder

        public TransportMessage Decode(byte[] data)
        {
            return _serializer.Deserialize<byte[], TransportMessage>(data);
        }

        #endregion Implementation of ITransportMessageDecoder
    }
}