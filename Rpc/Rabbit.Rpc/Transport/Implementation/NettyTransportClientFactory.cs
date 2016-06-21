using Rabbit.Rpc.Serialization;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;

namespace Rabbit.Rpc.Transport.Implementation
{
    /// <summary>
    /// 基于Netty传输客户端的工厂实现。
    /// </summary>
    public class NettyTransportClientFactory : ITransportClientFactory, IDisposable
    {
        #region Field

        private readonly ISerializer _serialization;
        private readonly ConcurrentDictionary<string, Lazy<ITransportClient>> _clients = new ConcurrentDictionary<string, Lazy<ITransportClient>>();

        #endregion Field

        #region Constructor

        public NettyTransportClientFactory(ISerializer serialization)
        {
            _serialization = serialization;
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
            return _clients.GetOrAdd(endPoint.ToString(), key => new Lazy<ITransportClient>(() => new NettyTransportClient(endPoint, _serialization))).Value;
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