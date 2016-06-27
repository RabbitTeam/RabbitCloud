using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Rabbit.Rpc.Logging;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Serialization;
using Rabbit.Rpc.Server;
using Rabbit.Rpc.Server.Implementation;
using Rabbit.Rpc.Transport;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Rabbit.Transport.DotNetty
{
    /// <summary>
    /// 默认的服务主机实现。
    /// </summary>
    public class DotNettyServiceHost : ServiceHostAbstract
    {
        #region Field

        private readonly ILogger<DotNettyServiceHost> _logger;
        private readonly ISerializer<byte[]> _serializer;
        private IChannel _channel;

        #endregion Field

        #region Constructor

        public DotNettyServiceHost(IServiceExecutor serviceExecutor, ILogger<DotNettyServiceHost> logger, ISerializer<byte[]> serializer) : base(serviceExecutor)
        {
            _logger = logger;
            _serializer = serializer;
        }

        #endregion Constructor

        #region Overrides of ServiceHostAbstract

        /// <summary>
        /// 启动主机。
        /// </summary>
        /// <param name="endPoint">主机终结点。</param>
        /// <returns>一个任务。</returns>
        public override async Task StartAsync(EndPoint endPoint)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.Debug($"准备启动服务主机，监听地址：{endPoint}。");

            var bossGroup = new MultithreadEventLoopGroup(1);
            var workerGroup = new MultithreadEventLoopGroup();
            var bootstrap = new ServerBootstrap();
            bootstrap
                .Group(bossGroup, workerGroup)
                .Channel<TcpServerSocketChannel>()
                .Option(ChannelOption.SoBacklog, 100)
                .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    pipeline.AddLast(new LengthFieldPrepender(4));
                    pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                    pipeline.AddLast(new TransportMessageChannelHandlerAdapter(_serializer));
                    pipeline.AddLast(new ServerHandler(MessageListener, _logger, _serializer));
                }));
            _channel = await bootstrap.BindAsync(endPoint);

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.Debug($"服务主机启动成功，监听地址：{endPoint}。");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            Task.Run(async () =>
            {
                await _channel.DisconnectAsync();
            }).Wait();
        }

        #endregion Overrides of ServiceHostAbstract

        #region Help Class

        private class ServerHandler : ChannelHandlerAdapter
        {
            private readonly IMessageListener _messageListener;
            private readonly ILogger _logger;
            private readonly ISerializer<byte[]> _serializer;

            public ServerHandler(IMessageListener messageListener, ILogger logger, ISerializer<byte[]> serializer)
            {
                _messageListener = messageListener;
                _logger = logger;
                _serializer = serializer;
            }

            #region Overrides of ChannelHandlerAdapter

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var transportMessage = (TransportMessage)message;

                _messageListener.OnReceived(new DotNettyServerMessageSender(_serializer, context), transportMessage);
            }

            public override void ChannelReadComplete(IChannelHandlerContext context)
            {
                context.Flush();
            }

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.Error($"与服务器：{context.Channel.RemoteAddress}通信时发送了错误。", exception);
            }

            #endregion Overrides of ChannelHandlerAdapter
        }

        #endregion Help Class
    }
}