/*using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Rabbit.Rpc.Logging;
using Rabbit.Rpc.Serialization;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Transport.Implementation
{
    /// <summary>
    /// 基于Netty的传输客户端实现。
    /// </summary>
    public class NettyTransportClient : ITransportClient, IDisposable
    {
        #region Field

        private readonly EndPoint _endPoint;
        private readonly ISerializer _serialization;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<TransportMessage>> _resultDictionary = new ConcurrentDictionary<string, TaskCompletionSource<TransportMessage>>();

        private readonly Task<IChannel> _channel;

        #endregion Field

        #region Constructor

        public NettyTransportClient(EndPoint endPoint, ISerializer serialization, ILogger logger)
        {
            _endPoint = endPoint;
            _serialization = serialization;
            _logger = logger;
            _channel = ConnectAsync();
        }

        #endregion Constructor

        #region Implementation of ITransportClient

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">消息模型。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAsync(TransportMessage message)
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.Debug("准备发送消息。");
                var data = _serialization.Serialize(message);
                var buffer = Unpooled.Buffer(data.Length);
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.Debug($"数据包大小为：{data.Length}。");
                buffer.WriteBytes(data);
                var channel = await _channel;
                await channel.WriteAndFlushAsync(buffer);
            }
            catch (Exception exception)
            {
                if (_logger.IsEnabled(LogLevel.Fatal))
                    _logger.Fatal("消息发送失败。", exception);
                throw;
            }
        }

        /// <summary>
        /// 接受指定消息id的响应消息。
        /// </summary>
        /// <param name="id">消息Id。</param>
        /// <returns>一个任务。</returns>
        public async Task<TransportMessage> ReceiveAsync(string id)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.Debug($"准备获取Id为：{id}的响应内容。");
            TaskCompletionSource<TransportMessage> task;
            if (_resultDictionary.ContainsKey(id))
            {
                if (_resultDictionary.TryRemove(id, out task))
                {
                    return await task.Task;
                }
            }
            else
            {
                task = new TaskCompletionSource<TransportMessage>();
                _resultDictionary.TryAdd(id, task);
                return await task.Task;
            }
            return null;
        }

        #endregion Implementation of ITransportClient

        #region Private Method

        private async Task<IChannel> ConnectAsync()
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.Information($"准备连接到服务器：{_endPoint}。");

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

                    pipeline.AddLast(new MessageReceiveHandler(_serialization, _resultDictionary, _logger));
                }));
            var result = await bootstrap.ConnectAsync(_endPoint);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.Information($"连接到服务器：{_endPoint}，成功。");

            return result;
        }

        #endregion Private Method

        #region Help Class

        private class MessageReceiveHandler : ChannelHandlerAdapter
        {
            private readonly ISerializer _serialization;
            private readonly ConcurrentDictionary<string, TaskCompletionSource<TransportMessage>> _resultConcurrentDictionary;
            private readonly ILogger _logger;

            public MessageReceiveHandler(ISerializer serialization, ConcurrentDictionary<string, TaskCompletionSource<TransportMessage>> resultConcurrentDictionary, ILogger logger)
            {
                _serialization = serialization;
                _resultConcurrentDictionary = resultConcurrentDictionary;
                _logger = logger;
            }

            #region Overrides of ChannelHandlerAdapter

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var buffer = (IByteBuffer)message;

                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.Information($"接收到消息：{buffer.ToString(Encoding.UTF8)}。");

                TaskCompletionSource<TransportMessage> task;
                var content = buffer.ToArray();
                var result = _serialization.Deserialize<TransportMessage>(content);
                if (!_resultConcurrentDictionary.TryGetValue(result.Id, out task))
                    return;
                task.SetResult(result);
            }

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.Error($"与服务器：{context.Channel.RemoteAddress}通信时发送了错误。", exception);
            }

            #endregion Overrides of ChannelHandlerAdapter
        }

        #endregion Help Class

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Task.Run(async () =>
            {
                await (await _channel).CloseAsync();
            }).Wait();
        }

        #endregion Implementation of IDisposable
    }
}*/