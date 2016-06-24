using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Rabbit.Rpc.Logging;
using Rabbit.Rpc.Serialization;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Transport.Implementation
{
    public class TransportClientFactory : ITransportClientFactory, IDisposable
    {
        #region Field

        private readonly ISerializer<byte[]> _serializer;
        private readonly ILogger<TransportClientFactory> _logger;
        private readonly ConcurrentDictionary<string, Lazy<ITransportClient>> _clients = new ConcurrentDictionary<string, Lazy<ITransportClient>>();
        private Bootstrap _bootstrap;

        #endregion Field

        #region Constructor

        public TransportClientFactory(ISerializer<byte[]> serializer, ILogger<TransportClientFactory> logger)
        {
            _serializer = serializer;
            _logger = logger;
            _bootstrap = GetBootstrap();
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
                , k => new Lazy<ITransportClient>(() =>
                {
                    var messageListener = new MessageListener();

                    _bootstrap.Handler(new ActionChannelInitializer<ISocketChannel>(c =>
                    {
                        var pipeline = c.Pipeline;
                        pipeline.AddLast(new LengthFieldPrepender(4));
                        pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                        pipeline.AddLast(new DefaultChannelHandler(messageListener));
                    }));

                    var bootstrap = _bootstrap;
                    var channel = bootstrap.ConnectAsync(endPoint);
                    var messageSender = new NettyMessageClientSender(channel);
                    var client = new TransportClient(messageSender, messageListener, _logger, _serializer);
                    return client;
                }
                )).Value;
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

        private static Bootstrap GetBootstrap()
        {
            var bootstrap = new Bootstrap();
            bootstrap
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Group(new MultithreadEventLoopGroup());

            return bootstrap;
        }

        protected class DefaultChannelHandler : ChannelHandlerAdapter
        {
            private readonly IMessageListener _messageListener;

            public DefaultChannelHandler(IMessageListener messageListener)
            {
                _messageListener = messageListener;
            }

            #region Overrides of ChannelHandlerAdapter

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                _messageListener.OnReceived(new NettyMessageClientSender(Task.FromResult(context.Channel)), message);
            }

            #endregion Overrides of ChannelHandlerAdapter
        }
    }
}