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
        /// <returns>一个任务。</returns>
        public async Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            if (Received == null)
                return;
            await Received(sender, message);
        }

        #endregion Implementation of IMessageListener

        public async Task StartAsync(EndPoint endPoint)
        {
            _server = new TcpSocketSaeaServer((IPEndPoint)endPoint, async (session, data, offset, count) =>
            {
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("接收到数据包。");
                var message = _transportMessageDecoder.Decode(data.Skip(offset).Take(count).ToArray());
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("接收到消息：" + message.Id);
                var sender = new SimpleServerMessageSender(_transportMessageEncoder, session, _logger);
                await OnReceived(sender, message);
            });
            _server.Listen();

#if NET
            await Task.FromResult(1);
#else
            await Task.CompletedTask;
#endif
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