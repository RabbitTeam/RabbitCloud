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
    public class NettyTransportChannel : ITransportChannel
    {
        private readonly ILogger _logger;

        private readonly TaskCompletionSource<IChannel> _channelCompletionSource = new TaskCompletionSource<IChannel>();
        private Task<IChannel> Channel => _channelCompletionSource.Task;

        public NettyTransportChannel(ILogger logger)
        {
            _logger = logger;
        }

        #region Implementation of ITransportChannel

        private ReceivedDelegate _receivedDelegate;

        public event ReceivedDelegate Received
        {
            add { _receivedDelegate += value; }
            remove { _receivedDelegate -= value; }
        }

        /// <summary>
        /// 是否打开。
        /// </summary>
        public bool Open => _channelCompletionSource.Task.IsCompleted && _channelCompletionSource.Task.Result.Open;

        /// <summary>
        /// 本地地址。
        /// </summary>
        public EndPoint LocalAddress => _channelCompletionSource.Task.IsCompleted ? _channelCompletionSource.Task.Result.LocalAddress : null;

        /// <summary>
        /// 远程地址。
        /// </summary>
        public EndPoint RemoteAddress => _channelCompletionSource.Task.IsCompleted ? _channelCompletionSource.Task.Result.RemoteAddress : null;

        /// <summary>
        /// 连接到远程服务器。
        /// </summary>
        /// <param name="remoteAddress">远程服务器地址。</param>
        /// <returns>一个任务。</returns>
        public Task ConnectAsync(EndPoint remoteAddress)
        {
            return ConnectAsync(remoteAddress, null);
        }

        protected class DefaultChannelHandler : ChannelHandlerAdapter
        {
            private readonly ITransportChannel _transportChannel;
            private readonly Action<ITransportChannel, object> _readAction;

            public DefaultChannelHandler(ITransportChannel transportChannel, Action<ITransportChannel, object> readAction)
            {
                _transportChannel = transportChannel;
                _readAction = readAction;
            }

            #region Overrides of ChannelHandlerAdapter

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                _readAction(_transportChannel, message);
            }

            public override void ChannelReadComplete(IChannelHandlerContext context)
            {
                context.Flush();
            }

            #endregion Overrides of ChannelHandlerAdapter
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

                    pipeline.AddLast(new DefaultChannelHandler(this, (c, m) =>
                    {
                        _receivedDelegate?.Invoke(c, m);
                    }));
                }));
            var result = await (localAddress == null ? bootstrap.ConnectAsync(remoteAddress) : bootstrap.ConnectAsync(remoteAddress, localAddress));

            _channelCompletionSource.SetResult(result);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.Information($"连接到服务器：{remoteAddress}，成功。");
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
            if (!_channelCompletionSource.Task.IsCompleted)
                return null;
            _channelCompletionSource.Task.Result.Flush();
            return this;
        }

        #endregion Implementation of ITransportChannel
    }
}