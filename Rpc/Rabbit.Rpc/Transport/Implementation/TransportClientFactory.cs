using Rabbit.Rpc.Logging;
using Rabbit.Rpc.Serialization;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;

namespace Rabbit.Rpc.Transport.Implementation
{
    public class TransportClientFactory : ITransportClientFactory, IDisposable
    {
        #region Field

        private readonly ISerializer<byte[]> _serializer;
        private readonly ILogger<TransportClientFactory> _logger;
        private readonly ConcurrentDictionary<string, Lazy<ITransportClient>> _clients = new ConcurrentDictionary<string, Lazy<ITransportClient>>();

        #endregion Field

        #region Constructor

        public TransportClientFactory(ISerializer<byte[]> serializer, ILogger<TransportClientFactory> logger)
        {
            _serializer = serializer;
            _logger = logger;
        }

        #endregion Constructor

        #region Implementation of ITransportClientFactory

        /// <summary>
        /// 创建客户端。
        /// </summary>
        /// <param name="endPoint">终结点。</param>
        /// <returns>传输客户端实例。</returns>
        public ITransportClient CreateClient(EndPoint endPoint)
        {
            var key = endPoint.ToString();
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.Debug($"准备为服务端地址：{key}创建客户端。");
            return _clients.GetOrAdd(key
                , k => new Lazy<ITransportClient>(() => new TransportClient(endPoint, _logger, _serializer))).Value;
        }

        #endregion Implementation of ITransportClientFactory

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
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