using Rabbit.Rpc.Codec.ProtoBuffer.Messages;
using Rabbit.Rpc.Codec.ProtoBuffer.Utilitys;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Transport.Codec;

namespace Rabbit.Rpc.Codec.ProtoBuffer
{
    public class ProtoBufferTransportMessageDecoder : ITransportMessageDecoder
    {
        #region Implementation of ITransportMessageDecoder

        public TransportMessage Decode(byte[] data)
        {
            var message = SerializerUtilitys.Deserialize<ProtoBufferTransportMessage>(data);

            return message.GetTransportMessage();
        }

        #endregion Implementation of ITransportMessageDecoder
    }
}