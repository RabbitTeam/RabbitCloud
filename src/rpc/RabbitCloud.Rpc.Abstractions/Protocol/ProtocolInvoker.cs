using RabbitCloud.Abstractions;
using System;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Protocol
{
    /// <summary>
    /// 协议调用者抽象类。
    /// </summary>
    public abstract class ProtocolInvoker : IInvoker
    {
        protected ProtocolInvoker(Url url)
        {
            Url = url;
        }

        /// <summary>
        /// 是否已经释放。
        /// </summary>
        public bool IsDisposed { get; private set; }

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

        #region Implementation of IInvoker

        /// <summary>
        /// 进行调用。
        /// </summary>
        /// <param name="invocation">调用信息。</param>
        /// <returns>返回结果。</returns>
        public async Task<IResult> Invoke(IInvocation invocation)
        {
            try
            {
                return await DoInvoke(invocation);
            }
            catch (Exception exception)
            {
                return new RpcResult(exception);
            }
        }

        #endregion Implementation of IInvoker

        #region Protected Method

        /// <summary>
        /// 进行调用。
        /// </summary>
        /// <param name="invocation">调用信息。</param>
        /// <returns>返回结果。</returns>
        protected abstract Task<IResult> DoInvoke(IInvocation invocation);

        #endregion Protected Method

        #region Implementation of IDisposable

        /// <summary>
        /// 执行与释放或重置非托管资源关联的应用程序定义的任务。
        /// </summary>
        public virtual void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            IsAvailable = false;
        }

        #endregion Implementation of IDisposable
    }
}