using RabbitCloud.Abstractions;

namespace RabbitCloud.Rpc.Abstractions.Internal
{
    /// <summary>
    /// 导出者抽象类。
    /// </summary>
    public abstract class Exporter : IExporter
    {
        protected Exporter(ICaller provider, Url url)
        {
            Provider = provider;
            Url = url;
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

        #region Implementation of IExporter

        /// <summary>
        /// 调用提供程序。
        /// </summary>
        public ICaller Provider { get; }

        #endregion Implementation of IExporter

        #region Implementation of IDisposable

        /// <summary>
        /// 执行与释放或重置非托管资源关联的应用程序定义的任务。
        /// </summary>
        public virtual void Dispose()
        {
            IsAvailable = false;
        }

        #endregion Implementation of IDisposable
    }
}