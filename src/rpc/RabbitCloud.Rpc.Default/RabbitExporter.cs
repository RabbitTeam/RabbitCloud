using RabbitCloud.Rpc.Abstractions;

namespace RabbitCloud.Rpc.Default
{
    public class RabbitExporter : IExporter
    {
        public RabbitExporter(IInvoker invoker)
        {
            Invoker = invoker;
        }

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
        }

        #endregion Implementation of IDisposable

        #region Implementation of IExporter

        public IInvoker Invoker { get; }

        #endregion Implementation of IExporter
    }
}