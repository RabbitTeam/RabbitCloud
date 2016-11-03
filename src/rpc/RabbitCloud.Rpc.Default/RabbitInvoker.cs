using RabbitCloud.Abstractions;
using RabbitCloud.Abstractions.Feature;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Protocol;
using RabbitCloud.Rpc.Default.Service;
using RabbitCloud.Rpc.Default.Service.Message;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Default
{
    public class RabbitInvoker : ProtocolInvoker
    {
        private readonly ClientEntry _clientEntry;

        public RabbitInvoker(Url url, ClientEntry clientEntry) : base(url)
        {
            _clientEntry = clientEntry;
        }

        #region Overrides of ProtocolInvoker

        protected override async Task<IResult> DoInvoke(IInvocation invocation)
        {
            invocation.Attributes.SetMetadata("path", Url.Path);
            var requestMessage = RequestMessage.Create((RpcInvocation)invocation);

            var responseMessage = await _clientEntry.Send(requestMessage);

            return responseMessage.Exception == null
                ? new RpcResult(responseMessage.Result)
                : new RpcResult(responseMessage.Exception);
        }

        /// <summary>
        /// 执行与释放或重置非托管资源关联的应用程序定义的任务。
        /// </summary>
        public override void Dispose()
        {
            _clientEntry.Dispose();
        }

        #endregion Overrides of ProtocolInvoker
    }
}