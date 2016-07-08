using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Runtime.Server;
using Rabbit.Rpc.Transport;
using Rabbit.Rpc.Transport.Codec;
using Rabbit.Rpc.Transport.Implementation;
using Rabbit.Transport.Simple.Tcp.Client;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Rabbit.Transport.Simple
{
    public class SimpleTransportClientFactory : ITransportClientFactory, IDisposable
    {
        private readonly ITransportMessageCodecFactory _transportMessageCodecFactory;
        private readonly ILogger<SimpleTransportClientFactory> _logger;
        private readonly IServiceExecutor _serviceExecutor;
        private readonly ConcurrentDictionary<string, Lazy<ITransportClient>> _clients = new ConcurrentDictionary<string, Lazy<ITransportClient>>();

        public SimpleTransportClientFactory(ITransportMessageCodecFactory transportMessageCodecFactory, ILogger<SimpleTransportClientFactory> logger) : this(transportMessageCodecFactory, logger, null)
        {
        }

        public SimpleTransportClientFactory(ITransportMessageCodecFactory transportMessageCodecFactory, ILogger<SimpleTransportClientFactory> logger, IServiceExecutor serviceExecutor)
        {
            _transportMessageCodecFactory = transportMessageCodecFactory;
            _logger = logger;
            _serviceExecutor = serviceExecutor;
        }

        #region Implementation of ITransportClientFactory

        /// <summary>
        /// 创建客户端。
        /// </summary>
        /// <param name="endPoint">终结点。</param>
        /// <returns>传输客户端实例。</returns>
        public ITransportClient CreateClient(EndPoint endPoint)
        {
            var config = new TcpSocketSaeaClientConfiguration();
            var key = endPoint.ToString();
            return _clients.GetOrAdd(key, k => new Lazy<ITransportClient>(() =>
            {
                var messageListener = new MessageListener();
                var client = new TcpSocketSaeaClient((IPEndPoint)endPoint, new SimpleMessageDispatcher(messageListener, _transportMessageCodecFactory, _logger), config);
                client.Connect().Wait();
                return new TransportClient(new SimpleClientMessageSender(_transportMessageCodecFactory.GetEncoder(), client), messageListener, _logger, _serviceExecutor);
            })).Value;
        }

        #endregion Implementation of ITransportClientFactory

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            foreach (var client in _clients.Values.Where(i => i.IsValueCreated))
            {
                (client.Value as IDisposable)?.Dispose();
            }
        }

        #endregion Implementation of IDisposable
    }

    internal class SimpleMessageDispatcher : ITcpSocketSaeaClientMessageDispatcher
    {
        private readonly IMessageListener _messageListener;
        private readonly ILogger _logger;
        private readonly ITransportMessageEncoder _transportMessageEncoder;
        private readonly ITransportMessageDecoder _transportMessageDecoder;

        public SimpleMessageDispatcher(IMessageListener messageListener, ITransportMessageCodecFactory transportMessageCodecFactory, ILogger logger)
        {
            _messageListener = messageListener;
            _logger = logger;
            _transportMessageEncoder = transportMessageCodecFactory.GetEncoder();
            _transportMessageDecoder = transportMessageCodecFactory.GetDecoder();
        }

        #region Implementation of ITcpSocketSaeaClientMessageDispatcher

        public Task OnServerConnected(TcpSocketSaeaClient client)
        {
#if NET
            return Task.FromResult(1);
#else
            return Task.CompletedTask;
#endif
        }

        public async Task OnServerDataReceived(TcpSocketSaeaClient client, byte[] data, int offset, int count)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("接收到数据包。");

            var message = _transportMessageDecoder.Decode(data.Skip(offset).Take(count).ToArray());

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("接收到消息：" + message.Id);

            _messageListener.OnReceived(new SimpleClientMessageSender(_transportMessageEncoder, client), message);

#if NET
            await Task.FromResult(1);
#else
            await Task.CompletedTask;
#endif
        }

        public Task OnServerDisconnected(TcpSocketSaeaClient client)
        {
#if NET
            return Task.FromResult(1);
#else
            return Task.CompletedTask;
#endif
        }

        #endregion Implementation of ITcpSocketSaeaClientMessageDispatcher
    }
}