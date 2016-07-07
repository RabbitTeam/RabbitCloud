using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Transport;
using Rabbit.Rpc.Transport.Codec;
using Rabbit.Transport.Simple.Tcp.Server;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Rabbit.Transport.Simple
{
    public class SimpleServerMessageListener : IMessageListener, IDisposable
    {
        #region Field

        private readonly ITransportMessageDecoder _transportMessageDecoder;
        private readonly ITransportMessageEncoder _transportMessageEncoder;
        private readonly ILogger<SimpleServerMessageListener> _logger;

        private TcpSocketSaeaServer _server;

        #endregion Field

        #region Constructor

        public SimpleServerMessageListener(ITransportMessageCodecFactory codecFactory, ILogger<SimpleServerMessageListener> logger)
        {
            _transportMessageEncoder = codecFactory.GetEncoder();
            _transportMessageDecoder = codecFactory.GetDecoder();
            _logger = logger;
        }

        #endregion Constructor

        #region Implementation of IMessageListener

        public event ReceivedDelegate Received;

        /// <summary>
        /// 触发接收到消息事件。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="message">接收到的消息。</param>
        public void OnReceived(IMessageSender sender, TransportMessage message)
        {
            Received?.Invoke(sender, message);
        }

        #endregion Implementation of IMessageListener

        public async Task StartAsync(EndPoint endPoint)
        {
            var config = new TcpSocketSaeaServerConfiguration();
            _server = new TcpSocketSaeaServer((IPEndPoint)endPoint, new SimpleMessageDispatcher((session, message) =>
            {
                var sender = new SimpleServerMessageSender(_transportMessageEncoder, session, _logger);
                OnReceived(sender, message);
            }, _transportMessageDecoder, _logger), config);
            _server.Listen();

#if NET45 || NET451
            await Task.FromResult(1);
#else
            await Task.CompletedTask;
#endif
        }

        private class SimpleMessageDispatcher : ITcpSocketSaeaServerMessageDispatcher
        {
            private readonly Action<TcpSocketSaeaSession, TransportMessage> _readAction;
            private readonly ITransportMessageDecoder _transportMessageDecoder;
            private readonly ILogger _logger;

            public SimpleMessageDispatcher(Action<TcpSocketSaeaSession, TransportMessage> readAction, ITransportMessageDecoder transportMessageDecoder, ILogger logger)
            {
                _readAction = readAction;
                _transportMessageDecoder = transportMessageDecoder;
                _logger = logger;
            }

            #region Implementation of ITcpSocketSaeaServerMessageDispatcher

            public Task OnSessionStarted(TcpSocketSaeaSession session)
            {
#if NET45 || NET451
                return Task.FromResult(1);
#else
            return Task.CompletedTask;
#endif
            }

            public Task OnSessionDataReceived(TcpSocketSaeaSession session, byte[] data, int offset, int count)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("接收到数据包。");
                var message = _transportMessageDecoder.Decode(data.Skip(offset).Take(count).ToArray());
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("接收到消息：" + message.Id);
                _readAction(session, message);
#if NET45 || NET451
                return Task.FromResult(1);
#else
            return Task.CompletedTask;
#endif
            }

            public Task OnSessionClosed(TcpSocketSaeaSession session)
            {
#if NET45 || NET451
                return Task.FromResult(1);
#else
            return Task.CompletedTask;
#endif
            }

            #endregion Implementation of ITcpSocketSaeaServerMessageDispatcher
        }

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            _server.Dispose();
        }

        #endregion Implementation of IDisposable
    }
}