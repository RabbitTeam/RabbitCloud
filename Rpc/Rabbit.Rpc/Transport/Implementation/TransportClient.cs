using DotNetty.Buffers;
using Rabbit.Rpc.Logging;
using Rabbit.Rpc.Serialization;
using Rabbit.Rpc.Transport.Channels;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Transport.Implementation
{
    public class TransportClient : ITransportClient, IDisposable
    {
        private readonly ITransportChannel _transportChannel;
        private readonly ILogger _logger;
        private readonly ISerializer _serializer;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<TransportMessage>> _resultDictionary = new ConcurrentDictionary<string, TaskCompletionSource<TransportMessage>>();

        public TransportClient(ITransportChannel transportChannel, ILogger logger, ISerializer serializer)
        {
            _transportChannel = transportChannel;
            _logger = logger;
            _serializer = serializer;
            _transportChannel.Received += (c, message) =>
            {
                var buffer = (IByteBuffer)message;

                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.Information($"接收到消息：{buffer.ToString(Encoding.UTF8)}。");

                TaskCompletionSource<TransportMessage> task;
                var content = buffer.ToArray();
                var result = _serializer.Deserialize<TransportMessage>(content);
                if (!_resultDictionary.TryGetValue(result.Id, out task))
                    return;
                task.SetResult(result);
            };
        }

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

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            (_transportChannel as IDisposable)?.Dispose();
        }

        #endregion Implementation of IDisposable
    }
}