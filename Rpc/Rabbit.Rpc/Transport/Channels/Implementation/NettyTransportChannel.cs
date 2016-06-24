using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Rabbit.Rpc.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Transport.Channels.Implementation
{
    /// <summary>
    /// 基于Netty的传输通道。
    /// </summary>
    public class NettyTransportChannel : ITransportChannel
    {
        #region Field

        private readonly ILogger _logger;
        private readonly TaskCompletionSource<IChannel> _channelCompletionSource = new TaskCompletionSource<IChannel>();
        private ReceivedDelegate _receivedDelegate;
        private ExceptionCaughtDelegate _exceptionCaughtDelegate;

        #endregion Field

        #region Property

        private Task<IChannel> Channel => _channelCompletionSource.Task;

        #endregion Property

        #region Constructor

        public NettyTransportChannel(ILogger logger)
        {
            _logger = logger;
        }

        #endregion Constructor

        #region Implementation of ITransportChannel

        #region Event

        /// <summary>
        /// 接收到消息的事件。
        /// </summary>
        public event ReceivedDelegate Received
        {
            add { _receivedDelegate += value; }
            remove { _receivedDelegate -= value; }
        }

        /// <summary>
        /// 异常抓住事件。
        /// </summary>
        public event ExceptionCaughtDelegate ExceptionCaught
        {
            add { _exceptionCaughtDelegate += value; }
            remove { _exceptionCaughtDelegate -= value; }
        }

        #endregion Event

        /// <summary>
        /// 是否打开。
        /// </summary>
        public bool Open => Channel.IsCompleted && Channel.Result.Open;

        /// <summary>
        /// 本地地址。
        /// </summary>
        public EndPoint LocalAddress => Channel.IsCompleted ? Channel.Result.LocalAddress : null;

        /// <summary>
        /// 远程地址。
        /// </summary>
        public EndPoint RemoteAddress => Channel.IsCompleted ? Channel.Result.RemoteAddress : null;

        /// <summary>
        /// 连接到远程服务器。
        /// </summary>
        /// <param name="remoteAddress">远程服务器地址。</param>
        /// <returns>一个任务。</returns>
        public Task ConnectAsync(EndPoint remoteAddress)
        {
            return ConnectAsync(remoteAddress, null);
        }

        /// <summary>
        /// 连接到远程服务器。
        /// </summary>
        /// <param name="remoteAddress">远程服务器地址。</param>
        /// <param name="localAddress">本地绑定地址。</param>
        /// <returns>一个任务。</returns>
        public async Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.Information($"准备连接到服务器：{remoteAddress}。");
            switch (Channel.Status)
            {
                case TaskStatus.WaitingForActivation:
                    var bootstrap = GetBootstrap();
                    try
                    {
                        var result = await bootstrap.ConnectAsync(remoteAddress, localAddress);
                        _channelCompletionSource.SetResult(result);
                    }
                    catch (Exception exception)
                    {
                        _channelCompletionSource.SetException(exception);
                        throw;
                    }
                    break;

                case TaskStatus.RanToCompletion:
                    var channel = await Channel;
                    try
                    {
                        await channel.DisconnectAsync();
                    }
                    catch
                    {
                        // ignored
                    }
                    await channel.ConnectAsync(remoteAddress, localAddress);
                    break;
            }
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.Information($"连接到服务器：{remoteAddress}，成功。");
        }

        /// <summary>
        /// 关闭通道。
        /// </summary>
        /// <returns>一个人任务。</returns>
        public async Task CloseAsync()
        {
            if (Channel.IsCompleted)
                await (await Channel).CloseAsync();
            else
                _channelCompletionSource.SetCanceled();
        }

        /// <summary>
        /// 断开频道。
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            if (Channel.IsCompleted)
                await (await Channel).DisconnectAsync();
            else
                _channelCompletionSource.SetCanceled();
        }

        /// <summary>
        /// 写入。
        /// </summary>
        /// <param name="message">消息对象。</param>
        /// <returns>一个人任务。</returns>
        public async Task WriteAsync(object message)
        {
            await (await Channel).WriteAsync(message);
        }

        /// <summary>
        /// 写入并刷新缓冲区。
        /// </summary>
        /// <param name="message">消息对象。</param>
        /// <returns>一个人任务。</returns>
        public async Task WriteAndFlushAsync(object message)
        {
            await (await Channel).WriteAndFlushAsync(message);
        }

        /// <summary>
        /// 刷新缓冲区。
        /// </summary>
        /// <returns>传输通道。</returns>
        public ITransportChannel Flush()
        {
            if (Channel.IsCompleted)
                Channel.Result.Flush();
            return this;
        }

        #endregion Implementation of ITransportChannel

        #region Private Method

        private Bootstrap GetBootstrap()
        {
            var bootstrap = new Bootstrap();
            bootstrap
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Group(new MultithreadEventLoopGroup())
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    pipeline.AddLast(new LengthFieldPrepender(4));
                    pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));

                    pipeline.AddLast(new DefaultChannelHandler(this, (c, m) => _receivedDelegate(c, m), (c, e) => _exceptionCaughtDelegate(c, e)));
                }));

            return bootstrap;
        }

        #endregion Private Method

        #region Help Class

        protected class DefaultChannelHandler : ChannelHandlerAdapter
        {
            private readonly ITransportChannel _transportChannel;
            private readonly ReceivedDelegate _channelRead;
            private readonly ExceptionCaughtDelegate _exceptionCaught;

            public DefaultChannelHandler(ITransportChannel transportChannel, ReceivedDelegate channelRead, ExceptionCaughtDelegate exceptionCaught)
            {
                _transportChannel = transportChannel;
                _channelRead = channelRead;
                _exceptionCaught = exceptionCaught;
            }

            #region Overrides of ChannelHandlerAdapter

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                _channelRead?.Invoke(_transportChannel, message);
            }

            public override void ChannelReadComplete(IChannelHandlerContext context)
            {
                context.Flush();
            }

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                _exceptionCaught?.Invoke(_transportChannel, exception);
            }

            #endregion Overrides of ChannelHandlerAdapter
        }

        #endregion Help Class
    }
}