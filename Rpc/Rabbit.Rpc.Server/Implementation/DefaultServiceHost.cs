using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Rabbit.Rpc.Logging;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Serialization;
using Rabbit.Rpc.Transport;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Server.Implementation
{
    /// <summary>
    /// 默认的服务主机实现。
    /// </summary>
    public class DefaultServiceHost : IServiceHost
    {
        #region Field

        private readonly ISerializer _serializer;
        private readonly IServiceEntryLocate _serviceEntryLocate;
        private readonly ILogger<DefaultServiceHost> _logger;
        private IChannel _channel;

        #endregion Field

        #region Constructor

        public DefaultServiceHost(ISerializer serializer, IServiceEntryLocate serviceEntryLocate, ILogger<DefaultServiceHost> logger)
        {
            _serializer = serializer;
            _serviceEntryLocate = serviceEntryLocate;
            _logger = logger;
        }

        #endregion Constructor

        #region Implementation of IServiceHost

        /// <summary>
        /// 启动主机。
        /// </summary>
        /// <param name="endPoint">主机终结点。</param>
        /// <returns>一个任务。</returns>
        public async Task StartAsync(EndPoint endPoint)
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
                    pipeline.AddLast(new ServerHandler(_serializer, _serviceEntryLocate, _logger));
                }));
            _channel = await bootstrap.BindAsync(endPoint);

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.Debug($"服务主机启动成功，监听地址：{endPoint}。");
        }

        #endregion Implementation of IServiceHost

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Task.Run(async () =>
            {
                await _channel.CloseAsync();
            }).Wait();
        }

        #endregion Implementation of IDisposable

        #region Help Class

        private class ServerHandler : ChannelHandlerAdapter
        {
            private readonly ISerializer _serializer;
            private readonly IServiceEntryLocate _serviceEntryLocate;
            private readonly ILogger _logger;

            public ServerHandler(ISerializer serializer, IServiceEntryLocate serviceEntryLocate, ILogger logger)
            {
                _serializer = serializer;
                _serviceEntryLocate = serviceEntryLocate;
                _logger = logger;
            }

            #region Overrides of ChannelHandlerAdapter

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var buffer = (IByteBuffer)message;

                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.Information($"接收到消息：{buffer.ToString(Encoding.UTF8)}。");

                var content = buffer.ToArray();
                TransportMessage<RemoteInvokeMessage> transportMessage;
                try
                {
                    transportMessage = _serializer.Deserialize<TransportMessage<RemoteInvokeMessage>>(content);
                }
                catch (Exception exception)
                {
                    _logger.Error($"将接收到的消息反序列化成 TransportMessage<RemoteInvokeMessage> 时发送了错误，消息内容：{{buffer.ToString(Encoding.UTF8)}}。", exception);
                    return;
                }

                var invokeMessage = transportMessage.Content;
                var entry = _serviceEntryLocate.Locate(invokeMessage);

                if (entry == null)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.Error($"根据服务Id：{invokeMessage.ServiceId}，找不到服务条目。");
                    return;
                }

                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.Debug("准备执行本地逻辑。");

                object result;

                try
                {
                    result = entry.Func(invokeMessage.Parameters);
                }
                catch (Exception exception)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.Error("执行本地逻辑时候发生了错误。", exception);
                    result = null;
                }

                try
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.Debug("准备发送响应消息。");
                    var resultData = _serializer.Serialize(new TransportMessage
                    {
                        Content = result,
                        Id = transportMessage.Id
                    });

                    buffer = Unpooled.Buffer(resultData.Length);
                    buffer.WriteBytes(resultData);
                    context.WriteAsync(buffer);
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.Debug("响应消息发送成功。");
                }
                catch (Exception exception)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.Error("发送响应消息时候发生了异常。", exception);
                }
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