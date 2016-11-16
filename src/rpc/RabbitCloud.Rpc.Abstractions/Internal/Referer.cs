using RabbitCloud.Abstractions;
using System;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Internal
{
    /// <summary>
    /// RPC引用者抽象类。
    /// </summary>
    public abstract class Referer : ICaller
    {
        protected Referer(Type type, Url serviceUrl)
        {
            InterfaceType = type;
            Url = serviceUrl;
        }

        #region Implementation of INode

        /// <summary>
        /// 节点Url。
        /// </summary>
        public Url Url { get; }

        /// <summary>
        /// 是否可用。
        /// </summary>
        public bool IsAvailable { get; set; } = true;

        #endregion Implementation of INode

        #region Implementation of ICaller

        /// <summary>
        /// 接口类型。
        /// </summary>
        public Type InterfaceType { get; }

        /// <summary>
        /// 调用RPC请求。
        /// </summary>
        /// <param name="request">调用请求。</param>
        /// <returns>RPC请求响应结果。</returns>
        public Task<IResponse> Call(IRequest request)
        {
            return DoCall(request);
        }

        #endregion Implementation of ICaller

        #region Implementation of IDisposable

        /// <summary>
        /// 执行与释放或重置非托管资源关联的应用程序定义的任务。
        /// </summary>
        public void Dispose()
        {
            IsAvailable = false;
        }

        #endregion Implementation of IDisposable

        #region Protected Method

        /// <summary>
        /// 执行调用。
        /// </summary>
        /// <param name="request">RPC请求。</param>
        /// <returns>RPC响应。</returns>
        protected abstract Task<IResponse> DoCall(IRequest request);

        #endregion Protected Method
    }
}