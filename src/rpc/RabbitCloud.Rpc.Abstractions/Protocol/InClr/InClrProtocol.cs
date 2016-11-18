using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions.Internal;
using RabbitCloud.Rpc.Abstractions.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Protocol.InClr
{
    public class InClrProtocol : Protocol
    {
        #region Overrides of Protocol

        /// <summary>
        /// 协议的默认端口。
        /// </summary>
        public override int DefaultPort { get; } = 9983;

        /// <summary>
        /// 创建一个导出者。
        /// </summary>
        /// <param name="provider">RPC提供程序。</param>
        /// <param name="url">导出的Url。</param>
        /// <returns>服务导出者。</returns>
        protected override Task<IExporter> CreateExporter(ICaller provider, Url url)
        {
            return Task.FromResult<IExporter>(new InClrExporter(Exporters, provider, url));
        }

        /// <summary>
        /// 创建一个引用者。
        /// </summary>
        /// <param name="type">类型。</param>
        /// <param name="serviceUrl">服务Url。</param>
        /// <returns>服务引用者。</returns>
        protected override Task<ICaller> CreateReferer(Type type, Url serviceUrl)
        {
            return Task.FromResult<ICaller>(new InClrReferer(Exporters, type, serviceUrl));
        }

        #endregion Overrides of Protocol

        private class InClrExporter : Exporter
        {
            private readonly IDictionary<string, Lazy<Task<IExporter>>> _exporters;

            public InClrExporter(IDictionary<string, Lazy<Task<IExporter>>> exporters, ICaller provider, Url url) : base(provider, url)
            {
                _exporters = exporters;
            }

            #region Overrides of Exporter

            /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
            public override void Dispose()
            {
                var key = Url.GetProtocolKey();
                Lazy<Task<IExporter>> exporter;
                if (!_exporters.TryGetValue(key, out exporter))
                    return;
                if (exporter.IsValueCreated)
                    exporter.Value.Result.Dispose();
                _exporters.Remove(key);
            }

            #endregion Overrides of Exporter
        }

        private class InClrReferer : Referer
        {
            private readonly IDictionary<string, Lazy<Task<IExporter>>> _exporters;
            private Lazy<Task<IExporter>> _exporter;

            private Task<IExporter> Exporter
            {
                get
                {
                    if (_exporter != null)
                        return _exporter.Value;
                    var key = Url.GetProtocolKey();
                    _exporters.TryGetValue(key, out _exporter);
                    return _exporter.Value;
                }
            }

            public InClrReferer(IDictionary<string, Lazy<Task<IExporter>>> exporters, Type type, Url serviceUrl) : base(type, serviceUrl)
            {
                _exporters = exporters;
            }

            #region Overrides of Referer

            /// <summary>
            /// 执行调用。
            /// </summary>
            /// <param name="request">RPC请求。</param>
            /// <returns>RPC响应。</returns>
            protected override async Task<IResponse> DoCall(IRequest request)
            {
                return await (await Exporter).Provider.Call(request);
            }

            #endregion Overrides of Referer
        }
    }
}