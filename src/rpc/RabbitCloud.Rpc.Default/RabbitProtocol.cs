using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Protocol;
using RabbitCloud.Rpc.Default.Service;
using System;

namespace RabbitCloud.Rpc.Default
{
    public class RabbitProtocol : Protocol
    {
        private readonly IServerTable _serverTable;
        private readonly IClientTable _clientTable;

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
        protected override IExporter CreateExporter(ICaller provider, Url url)
        {
            _serverTable.OpenServer(url, key => Exporters[key].Value);
            return new RabbitExporter(provider, url);
        }

        /// <summary>
        /// 创建一个引用者。
        /// </summary>
        /// <param name="type">类型。</param>
        /// <param name="serviceUrl">服务Url。</param>
        /// <returns>服务引用者。</returns>
        protected override ICaller CreateReferer(Type type, Url serviceUrl)
        {
            return new RabbitInvoker(_clientTable, type, serviceUrl);
        }

        #endregion Overrides of Protocol
    }
}