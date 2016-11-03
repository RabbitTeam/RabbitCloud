namespace RabbitCloud.Rpc.Abstractions.Protocol
{
    /// <summary>
    /// 协议导出者抽象类。
    /// </summary>
    public class ProtocolExporter : IExporter
    {
        private bool _isDisposed;

        public ProtocolExporter(IInvoker invoker)
        {
            Invoker = invoker;
        }

        #region Implementation of IDisposable

        /// <summary>
        /// 执行与释放或重置非托管资源关联的应用程序定义的任务。
        /// </summary>
        public virtual void Dispose()
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
            Invoker?.Dispose();
        }

        #endregion Implementation of IDisposable

        #region Implementation of IExporter

        /// <summary>
        /// 调用者。
        /// </summary>
        public IInvoker Invoker { get; }

        #endregion Implementation of IExporter
    }
}