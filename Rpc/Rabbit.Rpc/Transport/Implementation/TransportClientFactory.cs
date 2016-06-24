using Rabbit.Rpc.Logging;
using Rabbit.Rpc.Serialization;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Rabbit.Rpc.Transport.Channels.Implementation;

namespace Rabbit.Rpc.Transport.Implementation
{
    public class TransportClientFactory : ITransportClientFactory, IDisposable
    {
        #region Field

        private readonly ISerializer _serializer;
        private readonly ILogger<TransportClientFactory> _logger;
        private readonly ConcurrentDictionary<string, Lazy<Task<ITransportClient>>> _clients = new ConcurrentDictionary<string, Lazy<Task<ITransportClient>>>();

        #endregion Field

        #region Constructor

        public TransportClientFactory(ISerializer serializer, ILogger<TransportClientFactory> logger)
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
        public async Task<ITransportClient> CreateClient(EndPoint endPoint)
        {
            var key = endPoint.ToString();
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.Debug($"准备为服务端地址：{key}创建客户端。");
            return await _clients.GetOrAdd(endPoint.ToString()
                , k =>
                new Lazy<Task<ITransportClient>>(
                    async () =>
                    {
                        var channel = new NettyTransportChannel(_logger);
                        await channel.ConnectAsync(endPoint);
                        return new TransportClient(channel, _logger, _serializer);
                    })).Value;
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