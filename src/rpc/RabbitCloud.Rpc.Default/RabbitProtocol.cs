using Microsoft.Extensions.Logging;
using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Protocol;
using RabbitCloud.Rpc.Abstractions.Utils.Extensions;
using RabbitCloud.Rpc.Default.Service;

namespace RabbitCloud.Rpc.Default
{
    public class RabbitProtocol : Protocol
    {
        private readonly IServerTable _serverTable;
        private readonly IClientTable _clientTable;

        public RabbitProtocol(IServerTable serverTable, IClientTable clientTable, ILogger logger) : base(logger)
        {
            _serverTable = serverTable;
            _clientTable = clientTable;
        }

        #region Overrides of Protocol

        /// <summary>
        /// 导出一个调用者。
        /// </summary>
        /// <param name="invoker">调用者。</param>
        /// <returns>导出者。</returns>
        public override IExporter Export(IInvoker invoker)
        {
            var serviceKey = invoker.Url.GetServiceKey();
            return Exporters.GetOrAdd(serviceKey, k =>
            {
                var exporter = new RabbitExporter(invoker, e =>
                {
                    IExporter value;
                    Exporters.TryRemove(k, out value);
                });
                _serverTable.OpenServer(invoker.Url, sk =>
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
            var client = _clientTable.OpenClient(url);
            return new RabbitInvoker(url, client);
        }

        #endregion Overrides of Protocol
    }
}