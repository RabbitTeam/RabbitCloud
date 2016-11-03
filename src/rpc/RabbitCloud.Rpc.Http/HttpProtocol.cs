using Microsoft.Extensions.Logging;
using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Protocol;
using RabbitCloud.Rpc.Abstractions.Utils.Extensions;
using RabbitCloud.Rpc.Http.Service;
using System.Net;
using System.Net.Http;

namespace RabbitCloud.Rpc.Http
{
    public class HttpProtocol : Protocol
    {
        private readonly IServiceTable _serviceTable;
        private readonly ICodec _codec;
        private static readonly HttpClient HttpClient;

        static HttpProtocol()
        {
            HttpClient = new HttpClient();
        }

        public HttpProtocol(IServiceTable serviceTable, ICodec codec, ILogger logger) : base(logger)
        {
            _serviceTable = serviceTable;
            _codec = codec;
        }

        #region Overrides of Protocol

        /// <summary>
        /// 导出一个调用者。
        /// </summary>
        /// <param name="invoker">调用者。</param>
        /// <returns>导出者。</returns>
        public override IExporter Export(IInvoker invoker)
        {
            var url = invoker.Url;
            var serviceKey = url.GetServiceKey();
            return Exporters.GetOrAdd(serviceKey, k =>
            {
                var exporter = new HttpExporter(invoker, e =>
                {
                    IExporter value;
                    Exporters.TryRemove(k, out value);
                });

                var dnsEndPoint = new DnsEndPoint(url.Host, url.Port);
                _serviceTable.OpenServer(dnsEndPoint, sk =>
                {
                    IExporter value;
                    Exporters.TryGetValue(sk, out value);
                    return value;
                });
                return exporter;
            });
        }

        /// <summary>
        /// 引用一个调用者。
        /// </summary>
        /// <param name="url">调用者Url。</param>
        /// <returns>调用者。</returns>
        public override IInvoker Refer(Url url)
        {
            return new HttpInvoker(url, _codec, HttpClient);
        }

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public override void Dispose()
        {
            base.Dispose();
            HttpClient.Dispose();
            _serviceTable.Dispose();
        }

        #endregion Overrides of Protocol
    }
}