using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions.Utils.Extensions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

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
        protected ConcurrentDictionary<string, Lazy<Task<IExporter>>> Exporters { get; } = new ConcurrentDictionary<string, Lazy<Task<IExporter>>>(StringComparer.OrdinalIgnoreCase);

        #region Implementation of IProtocol

        /// <summary>
        /// 协议的默认端口。
        /// </summary>
        public abstract int DefaultPort { get; }

        /// <summary>
        /// 导出一个RPC提供程序。
        /// </summary>
        /// <param name="provider">RPC提供程序。</param>
        /// <returns>一个导出者。</returns>
        public async Task<IExporter> Export(ICaller provider)
        {
            CheckUrl(provider.Url);

            var url = provider.Url;
            var protocolKey = url.GetProtocolKey();
            return await Exporters.GetOrAdd(protocolKey, new Lazy<Task<IExporter>>(() => CreateExporter(provider, url))).Value;
        }

        /// <summary>
        /// 引用一个RPC服务。
        /// </summary>
        /// <param name="type">本地服务类型。</param>
        /// <param name="serviceUrl">服务Url。</param>
        /// <returns>一个引用者。</returns>
        public async Task<ICaller> Refer(Type type, Url serviceUrl)
        {
            CheckUrl(serviceUrl);

            return await CreateReferer(type, serviceUrl);
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
                exporter.Result.Dispose();
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
        protected abstract Task<IExporter> CreateExporter(ICaller provider, Url url);

        /// <summary>
        /// 创建一个引用者。
        /// </summary>
        /// <param name="type">类型。</param>
        /// <param name="serviceUrl">服务Url。</param>
        /// <returns>服务引用者。</returns>
        protected abstract Task<ICaller> CreateReferer(Type type, Url serviceUrl);

        #endregion Public Method

        #region Private Method

        private void CheckUrl(Url url)
        {
            if (url.Port == -1)
                url.Port = DefaultPort;
        }

        #endregion Private Method
    }
}