using Microsoft.Extensions.Logging;
using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RabbitCloud.Rpc.Abstractions.Protocol
{
    /// <summary>
    /// 协议抽象类。
    /// </summary>
    public abstract class Protocol : IProtocol
    {
        #region Property

        protected ConcurrentDictionary<string, IExporter> Exporters { get; } =
            new ConcurrentDictionary<string, IExporter>(StringComparer.OrdinalIgnoreCase);

        protected HashSet<IInvoker> Invokers { get; } = new HashSet<IInvoker>();
        protected ILogger Logger { get; }

        #endregion Property

        protected Protocol(ILogger logger)
        {
            Logger = logger;
        }

        #region Implementation of IProtocol

        /// <summary>
        /// 导出一个调用者。
        /// </summary>
        /// <param name="invoker">调用者。</param>
        /// <returns>导出者。</returns>
        public abstract IExporter Export(IInvoker invoker);

        /// <summary>
        /// 引用一个调用者。
        /// </summary>
        /// <param name="url">调用者Url。</param>
        /// <returns>调用者。</returns>
        public abstract IInvoker Refer(Url url);

        #endregion Implementation of IProtocol

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public virtual void Dispose()
        {
            foreach (var invoker in Invokers)
            {
                if (invoker == null)
                    continue;
                try
                {
                    if (Logger.IsEnabled(LogLevel.Information))
                        Logger.LogInformation($"销毁引用: {invoker.Url}");
                    invoker.Dispose();
                }
                catch (Exception exception)
                {
                    Logger.LogWarning(default(EventId), exception, exception.Message);
                }
            }
            Invokers.Clear();

            foreach (var exporter in Exporters.Values)
            {
                if (exporter == null)
                    continue;
                try
                {
                    if (Logger.IsEnabled(LogLevel.Information))
                        Logger.LogInformation($"取消导出服务: {exporter.Invoker.Url}");
                    exporter.Dispose();
                }
                catch (Exception exception)
                {
                    Logger.LogWarning(default(EventId), exception, exception.Message);
                }
            }
            Exporters.Clear();
        }

        #endregion Implementation of IDisposable

        #region Protected Method

        /// <summary>
        /// 根据服务Url获取服务Key。
        /// </summary>
        /// <param name="url">服务Url。</param>
        /// <returns>服务Key。</returns>
        protected static string GetServiceKey(Url url)
        {
            return ProtocolUtils.GetServiceKey(url);
        }

        /// <summary>
        /// 获取服务Key。
        /// </summary>
        /// <param name="port">端口。</param>
        /// <param name="serviceName">服务名称。</param>
        /// <param name="serviceVersion">服务版本。</param>
        /// <param name="serviceGroup">服务组。</param>
        /// <returns>服务Key。</returns>
        protected static string GetServiceKey(int port, string serviceName, string serviceVersion, string serviceGroup)
        {
            return ProtocolUtils.GetServiceKey(port, serviceName, serviceVersion, serviceGroup);
        }

        #endregion Protected Method
    }
}