using DotNetty.Buffers;
using Rabbit.Rpc.Exceptions;
using Rabbit.Rpc.Logging;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Serialization;
using Rabbit.Rpc.Transport.Channels;
using Rabbit.Rpc.Transport.Channels.Implementation;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Transport.Implementation
{
    /// <summary>
    /// 一个默认的传输客户端实现。
    /// </summary>
    public class TransportClient : ITransportClient, IDisposable
    {
        #region Field

        private readonly ITransportChannel _transportChannel;
        private readonly ILogger _logger;
        private readonly ISerializer<byte[]> _serializer;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<RemoteInvokeResultMessage>> _resultDictionary = new ConcurrentDictionary<string, TaskCompletionSource<RemoteInvokeResultMessage>>();

        #endregion Field

        #region Constructor

        public TransportClient(EndPoint remoteAddress, ILogger logger, ISerializer<byte[]> serializer)
        {
            _transportChannel = new NettyTransportChannel(logger);
            _logger = logger;
            _serializer = serializer;
            _transportChannel.Received += _transportChannel_Received;
            _transportChannel.ConnectAsync(remoteAddress);
        }

        #endregion Constructor

        #region Implementation of ITransportClient

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">远程调用消息模型。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAsync(RemoteInvokeMessage message)
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.Debug("准备发送消息。");

                var data = _serializer.Serialize(message);
                var buffer = Unpooled.Buffer(data.Length);

                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.Debug($"数据包大小为：{data.Length}。");

                buffer.WriteBytes(data);
                await _transportChannel.WriteAndFlushAsync(buffer);
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
        /// <returns>远程调用结果消息模型。</returns>
        public async Task<RemoteInvokeResultMessage> ReceiveAsync(string id)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.Debug($"准备获取Id为：{id}的响应内容。");

            TaskCompletionSource<RemoteInvokeResultMessage> task;
            if (_resultDictionary.ContainsKey(id))
            {
                if (_resultDictionary.TryRemove(id, out task))
                {
                    await task.Task;
                }
            }
            else
            {
                task = new TaskCompletionSource<RemoteInvokeResultMessage>();
                _resultDictionary.TryAdd(id, task);
                return await task.Task;
            }
            return null;
        }

        #endregion Implementation of ITransportClient

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Task.Run(async () =>
            {
                await _transportChannel.DisconnectAsync();
            }).Wait();
            foreach (var taskCompletionSource in _resultDictionary.Values)
            {
                taskCompletionSource.TrySetCanceled();
            }
        }

        #endregion Implementation of IDisposable

        #region Private Method

        private void _transportChannel_Received(ITransportChannel channel, object message)
        {
            var buffer = (IByteBuffer)message;

            //todo:不一定是string类型的消息
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.Information($"接收到消息：{buffer.ToString(Encoding.UTF8)}。");

            TaskCompletionSource<RemoteInvokeResultMessage> task;
            var content = buffer.ToArray();
            var result = _serializer.Deserialize<byte[], RemoteInvokeResultMessage>(content);
            if (!_resultDictionary.TryGetValue(result.Id, out task))
                return;

            if (!string.IsNullOrEmpty(result.ExceptionMessage))
            {
                task.TrySetException(new RpcException(result.ExceptionMessage));
            }
            else
            {
                task.SetResult(result);
            }
        }

        #endregion Private Method
    }
}