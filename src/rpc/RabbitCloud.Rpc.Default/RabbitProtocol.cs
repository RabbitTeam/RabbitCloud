using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Utils.Extensions;
using RabbitCloud.Rpc.Default.Service;
using System.Collections.Concurrent;

namespace RabbitCloud.Rpc.Default
{
    public class RabbitProtocol : IProtocol
    {
        private readonly IServerTable _serverTable;
        private readonly IClientTable _clientTable;
        private readonly ConcurrentDictionary<string, IExporter> _exporters = new ConcurrentDictionary<string, IExporter>();

        public RabbitProtocol(IServerTable serverTable, IClientTable clientTable)
        {
            _serverTable = serverTable;
            _clientTable = clientTable;
        }

        #region Implementation of IProtocol

        public IExporter Export(IInvoker invoker)
        {
            var serviceKey = invoker.Url.GetServiceKey();
            return _exporters.GetOrAdd(serviceKey, k =>
            {
                var exporter = new RabbitExporter(invoker, e =>
                {
                    IExporter value;
                    _exporters.TryRemove(k, out value);
                });
                _serverTable.OpenServer(invoker.Url.GetIpEndPoint().Result, sk =>
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
            var client = _clientTable.OpenClient(url.GetIpEndPoint().Result);
            return new RabbitInvoker(url, client);
        }

        #endregion Implementation of IProtocol
    }
}