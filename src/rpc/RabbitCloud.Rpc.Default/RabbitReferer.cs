using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Internal;
using RabbitCloud.Rpc.Default.Service;
using System;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Default
{
    public class RabbitReferer : Referer
    {
        private readonly IClientTable _clientTable;

        public RabbitReferer(IClientTable clientTable, Type type, Url serviceUrl) : base(type, serviceUrl)
        {
            _clientTable = clientTable;
        }

        #region Overrides of Referer

        /// <summary>
        /// 执行调用。
        /// </summary>
        /// <param name="request">RPC请求。</param>
        /// <returns>RPC响应。</returns>
        protected override async Task<IResponse> DoCall(IRequest request)
        {
            var client = _clientTable.OpenClient(Url);
            var response = await client.Send(request);
            return response;
        }

        #endregion Overrides of Referer
    }
}