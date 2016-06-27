using Rabbit.Rpc.Exceptions;
using Rabbit.Rpc.Logging;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Serialization;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Transport.Implementation
{
    /// <summary>
    /// 一个默认的传输客户端实现。
    /// </summary>
    public class TransportClient : ITransportClient, IDisposable
    {
        #region Field

        private readonly IMessageSender _messageSender;
        private readonly IMessageListener _messageListener;
        private readonly ILogger _logger;
        private readonly ISerializer<byte[]> _serializer;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<RemoteInvokeResultMessage>> _resultDictionary = new ConcurrentDictionary<string, TaskCompletionSource<RemoteInvokeResultMessage>>();

        #endregion Field

        #region Constructor

        public TransportClient(IMessageSender messageSender, IMessageListener messageListener, ILogger logger, ISerializer<byte[]> serializer)
        {
            _messageSender = messageSender;
            _messageListener = messageListener;
            _logger = logger;
            _serializer = serializer;
            messageListener.Received += MessageListener_Received;
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

                await _messageSender.SendAndFlushAsync(message);

                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.Debug("消息发送成功。");
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
            (_messageSender as IDisposable)?.Dispose();
            (_messageListener as IDisposable)?.Dispose();

            foreach (var taskCompletionSource in _resultDictionary.Values)
            {
                taskCompletionSource.TrySetCanceled();
            }
        }

        #endregion Implementation of IDisposable

        #region Private Method

        private void MessageListener_Received(IMessageSender sender, object message)
        {
            var buffer = (byte[])message;

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.Information("接收到消息。");

            TaskCompletionSource<RemoteInvokeResultMessage> task;
            var result = _serializer.Deserialize<byte[], RemoteInvokeResultMessage>(buffer);
            if (!_resultDictionary.TryGetValue(result.Id, out task))
                return;

            if (!string.IsNullOrEmpty(result.ExceptionMessage))
            {
                task.TrySetException(new RpcRemoteException(result.ExceptionMessage));
            }
            else
            {
                task.SetResult(result);
            }
        }

        #endregion Private Method
    }
}