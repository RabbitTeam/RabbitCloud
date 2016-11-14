using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Codec;
using RabbitCloud.Rpc.Abstractions.Utils.Extensions;
using System;
using System.Collections.Concurrent;

namespace RabbitCloud.Rpc.Default.Service
{
    public interface IServerTable : IDisposable
    {
        CowboyServer OpenServer(Url url, Func<string, IExporter> getExporter);
    }

    public class ServerTable : IServerTable
    {
        private readonly ICodec _codec;

        private readonly ConcurrentDictionary<string, Lazy<CowboyServer>> _serverEntries =
            new ConcurrentDictionary<string, Lazy<CowboyServer>>();

        public ServerTable(ICodec codec)
        {
            _codec = codec;
        }

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            foreach (var entry in _serverEntries.Values)
            {
                if (!entry.IsValueCreated)
                    continue;
                try
                {
                    entry.Value.Dispose();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            _serverEntries.Clear();
        }

        public CowboyServer OpenServer(Url url, Func<string, IExporter> getExporter)
        {
            var serverKey = $"{url.Host}:{url.Port}".ToLower();

            return _serverEntries.GetOrAdd(serverKey, new Lazy<CowboyServer>(() =>
            {
                var server = new CowboyServer(url, _codec, async request =>
                {
                    var protocolKey = url.GetProtocolKey();
                    var exporter = getExporter(protocolKey);
                    var response = await exporter.Provider.Call(request);
                    return response;
                    /*var invocation = request.Invocation;

                    var exporter = getExporter(ProtocolUtils.GetServiceKey(url));
                    var result = await exporter.Invoker.Invoke(invocation);
                    return ResponseMessage.Create(request, result.Value, result.Exception);*/
                });
                return server;
            })).Value;
        }

        #endregion Implementation of IDisposable
    }
}