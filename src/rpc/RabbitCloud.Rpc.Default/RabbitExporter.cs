using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Protocol;
using System;

namespace RabbitCloud.Rpc.Default
{
    public class RabbitExporter : ProtocolExporter
    {
        private readonly Action<IExporter> _dispose;

        public RabbitExporter(IInvoker invoker, Action<IExporter> dispose) : base(invoker)
        {
            _dispose = dispose;
        }

        #region Overrides of ProtocolExporter

        /// <summary>
        /// 执行与释放或重置非托管资源关联的应用程序定义的任务。
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            _dispose?.Invoke(this);
        }

        #endregion Overrides of ProtocolExporter
    }
}