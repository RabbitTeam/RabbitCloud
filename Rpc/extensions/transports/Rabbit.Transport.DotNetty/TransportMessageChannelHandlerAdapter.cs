using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Serialization;

namespace Rabbit.Transport.DotNetty
{
    internal class TransportMessageChannelHandlerAdapter : ChannelHandlerAdapter
    {
        private readonly ISerializer<byte[]> _serializer;

        public TransportMessageChannelHandlerAdapter(ISerializer<byte[]> serializer)
        {
            _serializer = serializer;
        }

        #region Overrides of ChannelHandlerAdapter

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var buffer = (IByteBuffer)message;
            var data = buffer.ToArray();
            var transportMessage = _serializer.Deserialize<byte[], TransportMessage>(data);
            context.FireChannelRead(transportMessage);
        }

        #endregion Overrides of ChannelHandlerAdapter
    }
}