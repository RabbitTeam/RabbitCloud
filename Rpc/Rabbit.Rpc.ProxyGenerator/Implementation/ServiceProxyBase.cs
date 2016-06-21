using Rabbit.Rpc.Client;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabbit.Rpc.ProxyGenerator.Implementation
{
    /// <summary>
    /// 一个抽象的服务代理基类。
    /// </summary>
    public abstract class ServiceProxyBase
    {
        #region Field

        private readonly IRemoteInvokeService _remoteInvokeService;
        private readonly ISerializer _serializer;

        #endregion Field

        #region Constructor

        protected ServiceProxyBase(IRemoteInvokeService remoteInvokeService, ISerializer serializer)
        {
            _remoteInvokeService = remoteInvokeService;
            _serializer = serializer;
        }

        #endregion Constructor

        #region Protected Method

        /// <summary>
        /// 远程调用。
        /// </summary>
        /// <typeparam name="T">返回类型。</typeparam>
        /// <param name="parameters">参数字典。</param>
        /// <param name="serviceId">服务Id。</param>
        /// <returns>调用结果。</returns>
        protected async Task<T> Invoke<T>(IDictionary<string, object> parameters, string serviceId)
        {
            var message = await _remoteInvokeService.InvokeAsync(new RemoteInvokeContext
            {
                InvokeMessage = new RemoteInvokeMessage
                {
                    Parameters = parameters,
                    ServiceId = serviceId
                }
            });

            var content = message.Content.ToString();
            var task = _serializer.Deserialize<TaskModel>(content);
            return (T)task.Result;
        }

        /// <summary>
        /// 远程调用。
        /// </summary>
        /// <param name="parameters">参数字典。</param>
        /// <param name="serviceId">服务Id。</param>
        /// <returns>调用任务。</returns>
        protected async Task Invoke(IDictionary<string, object> parameters, string serviceId)
        {
            await _remoteInvokeService.InvokeAsync(new RemoteInvokeContext
            {
                InvokeMessage = new RemoteInvokeMessage
                {
                    Parameters = parameters,
                    ServiceId = serviceId
                }
            });
        }

        #endregion Protected Method

        #region Help Class

        internal class TaskModel
        {
            public object Result { get; set; }
        }

        #endregion Help Class
    }
}