using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Rabbit.Rpc.Serialization;
using Rabbit.Rpc.Transport;
using System;
using System.Threading.Tasks;

namespace Rabbit.Transport.DotNetty
{
    /// <summary>
    /// 基于DotNetty的消息发送者基类。
    /// </summary>
    public abstract class DotNettyMessageSender
    {
        private readonly ISerializer<byte[]> _serializer;

        protected DotNettyMessageSender(ISerializer<byte[]> serializer)
        {
            _serializer = serializer;
        }

        protected IByteBuffer GetByteBuffer(object message)
        {
            var data = _serializer.Serialize(message);

            var buffer = Unpooled.Buffer(data.Length, data.Length);
            return buffer.WriteBytes(data);
        }
    }

    /// <summary>
    /// 基于DotNetty客户端的消息发送者。
    /// </summary>
    public class DotNettyMessageClientSender : DotNettyMessageSender, IMessageSender, IDisposable
    {
        private readonly Task<IChannel> _channel;

        public DotNettyMessageClientSender(ISerializer<byte[]> serializer, Task<IChannel> channel) : base(serializer)
        {
            _channel = channel;
        }

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Task.Run(async () =>
            {
                await (await _channel).DisconnectAsync();
            }).Wait();
        }

        #endregion Implementation of IDisposable

        #region Implementation of IMessageSender

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAsync(object message)
        {
            var buffer = GetByteBuffer(message);
            await (await _channel).WriteAsync(buffer);
        }

        /// <summary>
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAndFlushAsync(object message)
        {
            var buffer = GetByteBuffer(message);
            await (await _channel).WriteAndFlushAsync(buffer);
        }

        #endregion Implementation of IMessageSender
    }

    /// <summary>
    /// 基于DotNetty服务端的消息发送者。
    /// </summary>
    public class DotNettyServerMessageSender : DotNettyMessageSender, IMessageSender
    {
        private readonly IChannelHandlerContext _context;

        public DotNettyServerMessageSender(ISerializer<byte[]> serializer, IChannelHandlerContext context) : base(serializer)
        {
            _context = context;
        }

        #region Implementation of IMessageSender

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public Task SendAsync(object message)
        {
            var buffer = GetByteBuffer(message);
            return _context.WriteAsync(buffer);
        }

        /// <summary>
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public Task SendAndFlushAsync(object message)
        {
            var buffer = GetByteBuffer(message);
            return _context.WriteAndFlushAsync(buffer);
        }

        #endregion Implementation of IMessageSender
    }
}