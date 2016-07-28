using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Exceptions;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Runtime.Server;
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
        private readonly IServiceExecutor _serviceExecutor;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<TransportMessage>> _resultDictionary =
            new ConcurrentDictionary<string, TaskCompletionSource<TransportMessage>>();

        #endregion Field

        #region Constructor

        public TransportClient(IMessageSender messageSender, IMessageListener messageListener, ILogger logger,
            IServiceExecutor serviceExecutor)
        {
            _messageSender = messageSender;
            _messageListener = messageListener;
            _logger = logger;
            _serviceExecutor = serviceExecutor;
            messageListener.Received += MessageListener_Received;
        }

        #endregion Constructor

        #region Implementation of ITransportClient

        /// <summary>
        /// 发送传输消息。
        /// </summary>
        /// <param name="message">传输消息。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAsync(TransportMessage message)
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("准备发送消息。");

                try
                {
                    //发送
                    await _messageSender.SendAndFlushAsync(message);

                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug("消息发送成功。");
                }
                catch (Exception exception)
                {
                    throw new RpcCommunicationException("与服务端通讯时发生了异常。", exception);
                }
            }
            catch (Exception exception)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError("消息发送失败。", exception);
                throw;
            }
        }

        /// <summary>
        /// 根据消息Id接收一个传输消息。
        /// </summary>
        /// <param name="messageId">消息Id。</param>
        /// <returns>传输消息。</returns>
        public async Task<TransportMessage> ReceiveAsync(string messageId)
        {
            var taskCompletionSource = _resultDictionary.GetOrAdd(messageId, k => new TaskCompletionSource<TransportMessage>());
            try
            {
                return await taskCompletionSource.Task;
            }
            finally
            {
                //删除回调任务
                TaskCompletionSource<TransportMessage> value;
                _resultDictionary.TryRemove(messageId, out value);
            }
        }

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">远程调用消息模型。</param>
        /// <returns>远程调用消息的传输消息。</returns>
        public async Task<TransportMessage> SendAsync(RemoteInvokeMessage message)
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("准备发送消息。");

                var transportMessage = TransportMessage.CreateInvokeMessage(message);

                try
                {
                    //发送
                    await _messageSender.SendAndFlushAsync(transportMessage);

                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug("消息发送成功。");
                    return transportMessage;
                }
                catch (Exception exception)
                {
                    throw new RpcCommunicationException("与服务端通讯时发生了异常。", exception);
                }
            }
            catch (Exception exception)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError("消息发送失败。", exception);
                throw;
            }
        }

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">远程调用消息模型。</param>
        /// <returns>远程调用结果消息的传输消息。</returns>
        public async Task<RemoteInvokeResultMessage> SendAndWaitReturnAsync(RemoteInvokeMessage message)
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("准备发送消息。");

                var transportMessage = TransportMessage.CreateInvokeMessage(message);

                //注册结果回调
                var callbackTask = ReceiveAsync(transportMessage.Id);

                try
                {
                    //发送
                    await _messageSender.SendAndFlushAsync(transportMessage);
                }
                catch (Exception exception)
                {
                    throw new RpcCommunicationException("与服务端通讯时发生了异常。", exception);
                }

                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("消息发送成功。");

                var returnMessage = await callbackTask;
                return returnMessage.GetContent<RemoteInvokeResultMessage>();
            }
            catch (Exception exception)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError("消息发送失败。", exception);
                throw;
            }
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

        private async void MessageListener_Received(IMessageSender sender, TransportMessage message)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("接收到消息。");

            TaskCompletionSource<TransportMessage> task;
            if (!_resultDictionary.TryGetValue(message.Id, out task))
                return;

            if (message.IsInvokeResultMessage())
            {
                var content = message.GetContent<RemoteInvokeResultMessage>();
                if (!string.IsNullOrEmpty(content.ExceptionMessage))
                {
                    task.TrySetException(new RpcRemoteException(content.ExceptionMessage));
                }
                else
                {
                    task.SetResult(message);
                }
            }
            if (_serviceExecutor != null && message.IsInvokeMessage())
                await _serviceExecutor.ExecuteAsync(sender, message);
        }

        #endregion Private Method
    }
}