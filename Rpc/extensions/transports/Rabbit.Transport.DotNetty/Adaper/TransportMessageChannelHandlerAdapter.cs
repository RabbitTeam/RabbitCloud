using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Rabbit.Rpc.Transport.Codec;

namespace Rabbit.Transport.DotNetty.Adaper
{
    internal class TransportMessageChannelHandlerAdapter : ChannelHandlerAdapter
    {
        private readonly ITransportMessageDecoder _transportMessageDecoder;

        public TransportMessageChannelHandlerAdapter(ITransportMessageDecoder transportMessageDecoder)
        {
            _transportMessageDecoder = transportMessageDecoder;
        }

        #region Overrides of ChannelHandlerAdapter

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var buffer = (IByteBuffer)message;
            var data = buffer.ToArray();
            var transportMessage = _transportMessageDecoder.Decode(data);
            context.FireChannelRead(transportMessage);
        }

        #endregion Overrides of ChannelHandlerAdapter
    }
}