using DotNetty.Transport.Channels;
using System;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Transport.Implementation
{
    /// <summary>
    /// 基于Netty客户端的消息发送者。
    /// </summary>
    public class NettyMessageClientSender : IMessageSender, IDisposable
    {
        private readonly Task<IChannel> _channel;

        public NettyMessageClientSender(Task<IChannel> channel)
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
            await (await _channel).WriteAsync(message);
        }

        /// <summary>
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAndFlushAsync(object message)
        {
            await (await _channel).WriteAndFlushAsync(message);
        }

        #endregion Implementation of IMessageSender
    }

    /// <summary>
    /// 基于Netty服务端的消息发送者。
    /// </summary>
    public class NettyServerMessageSender : IMessageSender
    {
        private readonly IChannelHandlerContext _context;

        public NettyServerMessageSender(IChannelHandlerContext context)
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
            return _context.WriteAsync(message);
        }

        /// <summary>
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public Task SendAndFlushAsync(object message)
        {
            return _context.WriteAndFlushAsync(message);
        }

        #endregion Implementation of IMessageSender
    }
}