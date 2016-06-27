using Rabbit.Rpc.Messages;
using System;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Transport.Implementation
{
    /// <summary>
    /// 传输通道。
    /// </summary>
    public class TransportChannel : ITransportChannel, IDisposable
    {
        #region Field

        private readonly IMessageSender _messageSender;
        private readonly IMessageListener _messageListener = new MessageListener();

        #endregion Field

        #region Constructor

        public TransportChannel(IMessageSender messageSender)
        {
            _messageSender = messageSender;
        }

        #endregion Constructor

        #region Implementation of IMessageSender

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public Task SendAsync(TransportMessage message)
        {
            return _messageSender.SendAsync(message);
        }

        /// <summary>
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public Task SendAndFlushAsync(TransportMessage message)
        {
            return _messageSender.SendAndFlushAsync(message);
        }

        #endregion Implementation of IMessageSender

        #region Implementation of IMessageListener

        public event ReceivedDelegate Received;

        /// <summary>
        /// 触发接收到消息事件。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="message">接收到的消息。</param>
        public void OnReceived(IMessageSender sender, TransportMessage message)
        {
            _messageListener.OnReceived(sender, message);
        }

        #endregion Implementation of IMessageListener

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            (_messageListener as IDisposable)?.Dispose();
            (_messageSender as IDisposable)?.Dispose();
        }

        #endregion Implementation of IDisposable
    }
}