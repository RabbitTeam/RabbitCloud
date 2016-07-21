using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Transport;
using Rabbit.Rpc.Transport.Codec;
using Rabbit.Transport.Simple.Tcp;
using Rabbit.Transport.Simple.Tcp.Client;
using Rabbit.Transport.Simple.Tcp.Server;
using System;
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
        private readonly Func<TcpSocketSaeaClient> _clientFactory;
        private TcpSocketSaeaClient _client;

        private TcpSocketSaeaClient Client
        {
            get
            {
                if (_client != null && _client.State != TcpSocketConnectionState.Closed || _clientFactory == null)
                    return _client;
                lock (this)
                {
                    if (_client != null && _client.State != TcpSocketConnectionState.Closed || _clientFactory == null)
                        return _client;
                    return _client = _clientFactory();
                }
            }
        }

        public SimpleClientMessageSender(ITransportMessageEncoder transportMessageEncoder, Func<TcpSocketSaeaClient> clientFactory) : base(transportMessageEncoder)
        {
            _clientFactory = clientFactory;
        }

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
            await Client.SendAsync(data, 0, data.Length);
        }

        /// <summary>
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAndFlushAsync(TransportMessage message)
        {
            var data = GetByteBuffer(message);
            await Client.SendAsync(data, 0, data.Length);
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