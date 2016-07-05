using Rabbit.Rpc.Codec.ProtoBuffer.Messages;
using Rabbit.Rpc.Codec.ProtoBuffer.Utilitys;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Transport.Codec;

namespace Rabbit.Rpc.Codec.ProtoBuffer
{
    public class ProtoBufferTransportMessageEncoder : ITransportMessageEncoder
    {
        #region Implementation of ITransportMessageEncoder

        public byte[] Encode(TransportMessage message)
        {
            var transportMessage = new ProtoBufferTransportMessage(message)
            {
                Id = message.Id,
                ContentType = message.ContentType
            };

            return SerializerUtilitys.Serialize(transportMessage);
        }

        #endregion Implementation of ITransportMessageEncoder
    }
}