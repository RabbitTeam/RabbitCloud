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
            var key = endPoint.ToString();
            return _clients.GetOrAdd(key, k => new Lazy<ITransportClient>(() =>
            {
                var messageListener = new MessageListener();
                Func<TcpSocketSaeaClient> clientFactory = () =>
                {
                    var client = new TcpSocketSaeaClient((IPEndPoint)endPoint, async (c, data, offset, count) =>
                     {
                         if (_logger.IsEnabled(LogLevel.Information))
                             _logger.LogInformation("接收到数据包。");

                         var transportMessageDecoder = _transportMessageCodecFactory.GetDecoder();
                         var transportMessageEncoder = _transportMessageCodecFactory.GetEncoder();
                         var message = transportMessageDecoder.Decode(data.Skip(offset).Take(count).ToArray());

                         if (_logger.IsEnabled(LogLevel.Information))
                             _logger.LogInformation("接收到消息：" + message.Id);

                         await messageListener.OnReceived(new SimpleClientMessageSender(transportMessageEncoder, c), message);
                     });
                    client.Connect().Wait();
                    return client;
                };
                return new TransportClient(new SimpleClientMessageSender(_transportMessageCodecFactory.GetEncoder(), clientFactory), messageListener, _logger, _serviceExecutor);
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
}