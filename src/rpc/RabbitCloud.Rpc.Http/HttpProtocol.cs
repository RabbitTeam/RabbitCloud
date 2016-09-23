using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Utils.Extensions;
using RabbitCloud.Rpc.Http.Service;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;

namespace RabbitCloud.Rpc.Http
{
    public class HttpProtocol : IProtocol, IDisposable
    {
        private readonly IServiceTable _serviceTable;
        private readonly ConcurrentDictionary<string, IExporter> _exporters = new ConcurrentDictionary<string, IExporter>();
        private readonly ICodec _codec;
        private static readonly HttpClient HttpClient;

        static HttpProtocol()
        {
            HttpClient = new HttpClient();
        }

        public HttpProtocol(IServiceTable serviceTable, ICodec codec)
        {
            _serviceTable = serviceTable;
            _codec = codec;
        }

        #region Implementation of IProtocol

        public IExporter Export(IInvoker invoker)
        {
            var url = invoker.Url;
            var serviceKey = url.GetServiceKey();
            return _exporters.GetOrAdd(serviceKey, k =>
            {
                var exporter = new HttpExporter(invoker, e =>
                {
                    IExporter value;
                    _exporters.TryRemove(k, out value);
                });

                var dnsEndPoint = new DnsEndPoint(url.Host, url.Port);
                _serviceTable.OpenServer(dnsEndPoint, sk =>
                {
                    IExporter value;
                    _exporters.TryGetValue(sk, out value);
                    return value;
                });
                return exporter;
            });
        }

        public IInvoker Refer(Url url)
        {
            return new HttpInvoker(url, _codec, HttpClient);
        }

        #endregion Implementation of IProtocol

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            HttpClient.Dispose();
            foreach (var exporter in _exporters.Values)
                exporter.Dispose();
            _serviceTable.Dispose();
        }

        #endregion Implementation of IDisposable
    }
}