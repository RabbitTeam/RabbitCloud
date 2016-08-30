using RabbitCloud.Rpc.Abstractions;
using System;

namespace RabbitCloud.Rpc.Http
{
    public class HttpExporter : IExporter
    {
        private readonly Action<IExporter> _dispose;

        public HttpExporter(IInvoker invoker, Action<IExporter> dispose)
        {
            _dispose = dispose;
            Invoker = invoker;
        }

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            _dispose?.Invoke(this);
        }

        #endregion Implementation of IDisposable

        #region Implementation of IExporter

        public IInvoker Invoker { get; }

        #endregion Implementation of IExporter
    }
}