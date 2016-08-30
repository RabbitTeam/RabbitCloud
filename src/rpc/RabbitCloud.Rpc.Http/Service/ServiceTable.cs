using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Utils;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Http.Service
{
    public class ServerEntry : IDisposable
    {
        private readonly DnsEndPoint _dnsEndPoint;
        private readonly IWebHost _webHost;

        public delegate Task ReplyHandler(HttpContext context);

        private ReplyHandler _replyHandler;

        public event ReplyHandler Received
        {
            add { _replyHandler += value; }
            remove { throw new NotImplementedException(); }
        }

        private async Task OnReceived(HttpContext context)
        {
            if (_replyHandler == null)
                return;
            await _replyHandler(context);
        }

        public ServerEntry(DnsEndPoint dnsEndPoint)
        {
            _dnsEndPoint = dnsEndPoint;
            _webHost = CreateWebHost();
        }

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            _webHost.Dispose();
        }

        #endregion Implementation of IDisposable

        private IWebHost CreateWebHost()
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseUrls($"http://{_dnsEndPoint.Host}:{_dnsEndPoint.Port}")
                .Configure(app =>
                {
                    app.Run(OnReceived);
                })
                .Build();
            host.Start();
            return host;
        }
    }

    public interface IServiceTable : IDisposable
    {
        ServerEntry OpenServer(DnsEndPoint dnsEndPoint, Func<string, IExporter> getExporter);
    }

    public class ServiceTable : IServiceTable
    {
        private readonly ICodec _codec;

        private readonly ConcurrentDictionary<string, ServerEntry> _serverEntries =
            new ConcurrentDictionary<string, ServerEntry>();

        public ServiceTable(ICodec codec)
        {
            _codec = codec;
        }

        #region Implementation of IServiceTable

        public ServerEntry OpenServer(DnsEndPoint dnsEndPoint, Func<string, IExporter> getExporter)
        {
            var serverKey = dnsEndPoint.ToString();
            return _serverEntries.GetOrAdd(serverKey, k =>
            {
                var server = new ServerEntry(dnsEndPoint);

                server.Received += async context =>
                {
                    var httpConnectionFeature = context.Features.Get<IHttpConnectionFeature>();
                    var port = httpConnectionFeature.LocalPort;

                    var request = context.Request;

                    using (var reader = new StreamReader(request.Body))
                    {
                        var invocation = (Invocation)_codec.Decode(reader, typeof(Invocation));
                        var serviceKey = ProtocolUtils.GetServiceKey(port, request.Path, request.Query["version"],
                            request.Query["group"]);
                        var exporter = getExporter(serviceKey);
                        var result = await exporter.Invoker.Invoke(invocation);
                        var replyData = _codec.EncodeToBytes(result);
                        await context.Response.Body.WriteAsync(replyData, 0, replyData.Length);
                    }
                };
                return server;
            });
        }

        #endregion Implementation of IServiceTable

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            foreach (var serverEntriesValue in _serverEntries.Values)
            {
                try
                {
                    serverEntriesValue.Dispose();
                }
                catch
                {
                }
            }
        }

        #endregion Implementation of IDisposable
    }
}