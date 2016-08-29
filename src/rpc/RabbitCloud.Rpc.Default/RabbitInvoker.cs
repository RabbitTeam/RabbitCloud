using RabbitCloud.Abstractions;
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

        public RabbitInvoker(Id id, ClientEntry clientEntry) : base(id)
        {
            _clientEntry = clientEntry;
        }

        #region Overrides of ProtocolInvoker

        protected override async Task<IResult> DoInvoke(IInvocation invocation)
        {
            var requestMessage = RequestMessage.Create((Invocation)invocation);

            var responseMessage = await _clientEntry.Send(requestMessage);

            return responseMessage.Exception == null
                ? new Result(responseMessage.Result)
                : new Result(responseMessage.Exception);
        }

        #endregion Overrides of ProtocolInvoker
    }
}