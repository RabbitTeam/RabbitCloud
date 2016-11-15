using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions.Utils.Extensions;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace RabbitCloud.Rpc.Abstractions.Protocol
{
    /// <summary>
    /// 协议抽象类。
    /// </summary>
    public abstract class Protocol : IProtocol
    {
        /// <summary>
        /// 导出者字典表。
        /// </summary>
        protected ConcurrentDictionary<string, Lazy<IExporter>> Exporters { get; } = new ConcurrentDictionary<string, Lazy<IExporter>>(StringComparer.OrdinalIgnoreCase);

        #region Implementation of IProtocol

        /// <summary>
        /// 导出一个RPC提供程序。
        /// </summary>
        /// <param name="provider">RPC提供程序。</param>
        /// <returns>一个导出者。</returns>
        public IExporter Export(IProvider provider)
        {
            var url = provider.Url;
            var protocolKey = url.GetProtocolKey();
            return Exporters.GetOrAdd(protocolKey, new Lazy<IExporter>(() => CreateExporter(provider, url))).Value;
        }

        /// <summary>
        /// 引用一个RPC服务。
        /// </summary>
        /// <param name="type">本地服务类型。</param>
        /// <param name="serviceUrl">服务Url。</param>
        /// <returns>一个引用者。</returns>
        public IReferer Refer(Type type, Url serviceUrl)
        {
            return CreateReferer(type, serviceUrl);
        }

        #endregion Implementation of IProtocol

        #region Implementation of IDisposable

        /// <summary>
        /// 执行与释放或重置非托管资源关联的应用程序定义的任务。
        /// </summary>
        public virtual void Dispose()
        {
            foreach (var exporter in Exporters.Values.Where(i => i.IsValueCreated).Select(i => i.Value))
            {
                exporter.Dispose();
            }
            Exporters.Clear();
        }

        #endregion Implementation of IDisposable

        #region Public Method

        /// <summary>
        /// 创建一个导出者。
        /// </summary>
        /// <param name="provider">RPC提供程序。</param>
        /// <param name="url">导出的Url。</param>
        /// <returns>服务导出者。</returns>
        protected abstract IExporter CreateExporter(IProvider provider, Url url);

        /// <summary>
        /// 创建一个引用者。
        /// </summary>
        /// <param name="type">类型。</param>
        /// <param name="serviceUrl">服务Url。</param>
        /// <returns>服务引用者。</returns>
        protected abstract IReferer CreateReferer(Type type, Url serviceUrl);

        #endregion Public Method
    }
}