using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
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
        private IChannel _channel;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<TransportMessage>> _resultDictionary = new ConcurrentDictionary<string, TaskCompletionSource<TransportMessage>>();

        #endregion Field

        #region Constructor

        public NettyTransportClient(EndPoint endPoint, ISerializer serialization)
        {
            _endPoint = endPoint;
            _serialization = serialization;
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
            var content = _serialization.Serialize(message);
            var data = Encoding.UTF8.GetBytes(content);
            var buffer = Unpooled.Buffer(data.Length);
            buffer.WriteBytes(data);
            var channel = _channel = await ConnectAsync();
            await channel.WriteAndFlushAsync(buffer);
        }

        /// <summary>
        /// 接受指定消息id的响应消息。
        /// </summary>
        /// <param name="id">消息Id。</param>
        /// <returns>一个任务。</returns>
        public async Task<TransportMessage> ReceiveAsync(string id)
        {
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

        private Task<IChannel> ConnectAsync()
        {
            var bootstrap = new Bootstrap();
            bootstrap
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Group(new MultithreadEventLoopGroup())
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    pipeline.AddLast(new LengthFieldPrepender(2));
                    pipeline.AddLast(new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

                    pipeline.AddLast(new MessageReceiveHandler(_serialization, _resultDictionary));
                }));
            return bootstrap.ConnectAsync(_endPoint);
        }

        #endregion Private Method

        #region Help Class

        private class MessageReceiveHandler : ChannelHandlerAdapter
        {
            private readonly ISerializer _serialization;
            private readonly ConcurrentDictionary<string, TaskCompletionSource<TransportMessage>> _resultConcurrentDictionary;

            public MessageReceiveHandler(ISerializer serialization, ConcurrentDictionary<string, TaskCompletionSource<TransportMessage>> resultConcurrentDictionary)
            {
                _serialization = serialization;
                _resultConcurrentDictionary = resultConcurrentDictionary;
            }

            #region Overrides of ChannelHandlerAdapter

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var buffer = (IByteBuffer)message;
                var content = buffer.ToString(Encoding.UTF8);
                var result = _serialization.Deserialize<TransportMessage>(content);

                TaskCompletionSource<TransportMessage> task;
                if (_resultConcurrentDictionary.TryGetValue(result.Id, out task))
                {
                    task.SetResult(result);
                }
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
                await _channel.CloseAsync();
            }).Wait();
        }

        #endregion Implementation of IDisposable
    }
}