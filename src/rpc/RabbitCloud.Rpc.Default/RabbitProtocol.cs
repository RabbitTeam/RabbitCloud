using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Protocol;
using RabbitCloud.Rpc.Abstractions.Utils.Extensions;
using RabbitCloud.Rpc.Default.Service;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Default
{
    public class RabbitProtocol : Protocol
    {
        private readonly IServerTable _serverTable;
        private readonly IClientTable _clientTable;
        private readonly ConcurrentDictionary<string, string> _serviceKeyMappings = new ConcurrentDictionary<string, string>();

        public RabbitProtocol(IServerTable serverTable, IClientTable clientTable)
        {
            _serverTable = serverTable;
            _clientTable = clientTable;
        }

        #region Overrides of Protocol

        /// <summary>
        /// 创建一个导出者。
        /// </summary>
        /// <param name="provider">RPC提供程序。</param>
        /// <param name="url">导出的Url。</param>
        /// <returns>服务导出者。</returns>
        protected override Task<IExporter> CreateExporter(ICaller provider, Url url)
        {
            var serviceKey = url.GetServiceKey();
            if (!_serviceKeyMappings.ContainsKey(serviceKey))
                _serviceKeyMappings.TryAdd(serviceKey, url.GetProtocolKey());

            _serverTable.OpenServer(url, (u, request) =>
            {
                string protocolKey;
                _serviceKeyMappings.TryGetValue(request.GetServiceKey(), out protocolKey);
                return Exporters[protocolKey].Value.Result;
            });
            return Task.FromResult<IExporter>(new RabbitExporter(provider, url));
        }

        /// <summary>
        /// 创建一个引用者。
        /// </summary>
        /// <param name="type">类型。</param>
        /// <param name="serviceUrl">服务Url。</param>
        /// <returns>服务引用者。</returns>
        protected override Task<ICaller> CreateReferer(Type type, Url serviceUrl)
        {
            return Task.FromResult<ICaller>(new RabbitReferer(_clientTable, type, serviceUrl));
        }

        #endregion Overrides of Protocol
    }
}