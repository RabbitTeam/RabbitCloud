using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Transport;
using Rabbit.Rpc.Transport.Codec;
using Rabbit.Transport.Simple.Tcp.Client;
using Rabbit.Transport.Simple.Tcp.Server;
using System.Threading.Tasks;

namespace Rabbit.Transport.Simple
{
    public abstract class SimpleMessageSender
    {
        private readonly ITransportMessageEncoder _transportMessageEncoder;

        protected SimpleMessageSender(ITransportMessageEncoder transportMessageEncoder)
        {
            _transportMessageEncoder = transportMessageEncoder;
        }

        protected byte[] GetByteBuffer(TransportMessage message)
        {
            var data = _transportMessageEncoder.Encode(message);

            return data;
        }
    }

    public class SimpleClientMessageSender : SimpleMessageSender, IMessageSender
    {
        private readonly TcpSocketSaeaClient _client;

        public SimpleClientMessageSender(ITransportMessageEncoder transportMessageEncoder, TcpSocketSaeaClient client) : base(transportMessageEncoder)
        {
            _client = client;
        }

        #region Implementation of IMessageSender

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAsync(TransportMessage message)
        {
            var data = GetByteBuffer(message);
            await _client.SendAsync(data, 0, data.Length);
        }

        /// <summary>
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAndFlushAsync(TransportMessage message)
        {
            var data = GetByteBuffer(message);
            await _client.SendAsync(data, 0, data.Length);
        }

        #endregion Implementation of IMessageSender
    }

    public class SimpleServerMessageSender : SimpleMessageSender, IMessageSender
    {
        private readonly TcpSocketSaeaSession _session;
        private readonly ILogger _logger;

        public SimpleServerMessageSender(ITransportMessageEncoder transportMessageEncoder, TcpSocketSaeaSession session, ILogger logger) : base(transportMessageEncoder)
        {
            _session = session;
            _logger = logger;
        }

        #region Implementation of IMessageSender

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAsync(TransportMessage message)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("准备发送消息：" + message.Id);

            var data = GetByteBuffer(message);
            await _session.SendAsync(data, 0, data.Length);
        }

        /// <summary>
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAndFlushAsync(TransportMessage message)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("准备发送消息：" + message.Id);

            var data = GetByteBuffer(message);
            await _session.SendAsync(data, 0, data.Length);
        }

        #endregion Implementation of IMessageSender
    }
}