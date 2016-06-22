using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Rabbit.Rpc.Exceptions;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Serialization;
using Rabbit.Rpc.Transport;
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
        private IChannel _channel;

        #endregion Field

        #region Constructor

        public DefaultServiceHost(ISerializer serializer, IServiceEntryLocate serviceEntryLocate)
        {
            _serializer = serializer;
            _serviceEntryLocate = serviceEntryLocate;
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
                    pipeline.AddLast(new ServerHandler(_serializer, _serviceEntryLocate));
                }));
            _channel = await bootstrap.BindAsync(endPoint);
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

            public ServerHandler(ISerializer serializer, IServiceEntryLocate serviceEntryLocate)
            {
                _serializer = serializer;
                _serviceEntryLocate = serviceEntryLocate;
            }

            #region Overrides of ChannelHandlerAdapter

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var buffer = (IByteBuffer)message;
                var content = buffer.ToString(Encoding.UTF8);
                var model = _serializer.Deserialize<TransportMessage<RemoteInvokeMessage>>(content);

                var invokeMessage = model.Content;
                var entry = _serviceEntryLocate.Locate(invokeMessage);

                if (entry == null)
                    throw new RpcException($"根据服务Id：{invokeMessage.ServiceId}，找不到服务条目。");

                var result = entry.Func(invokeMessage.Parameters);

                var resultContent = _serializer.Serialize(new TransportMessage<object>
                {
                    Content = result,
                    Id = model.Id
                });
                var resultData = Encoding.UTF8.GetBytes(resultContent);

                buffer = Unpooled.Buffer(resultData.Length);
                buffer.WriteBytes(resultData);
                context.WriteAsync(buffer);
            }

            public override void ChannelReadComplete(IChannelHandlerContext context)
            {
                context.Flush();
            }

            #endregion Overrides of ChannelHandlerAdapter
        }

        #endregion Help Class
    }
}