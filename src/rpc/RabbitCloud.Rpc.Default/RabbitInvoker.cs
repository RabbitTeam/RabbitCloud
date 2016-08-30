using RabbitCloud.Abstractions;
using RabbitCloud.Abstractions.Feature;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Protocol;
using RabbitCloud.Rpc.Default.Service;
using RabbitCloud.Rpc.Default.Service.Message;
using System;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Default
{
    public class RabbitInvoker : ProtocolInvoker, IDisposable
    {
        private readonly ClientEntry _clientEntry;

        public RabbitInvoker(Url url, ClientEntry clientEntry) : base(url)
        {
            _clientEntry = clientEntry;
        }

        #region Overrides of ProtocolInvoker

        protected override async Task<IResult> DoInvoke(IInvocation invocation)
        {
            invocation.SetMetadata("path", Url.Path);
            var requestMessage = RequestMessage.Create((Invocation)invocation);

            var responseMessage = await _clientEntry.Send(requestMessage);

            return responseMessage.Exception == null
                ? new Result(responseMessage.Result)
                : new Result(responseMessage.Exception);
        }

        #endregion Overrides of ProtocolInvoker

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            _clientEntry.Dispose();
        }

        #endregion Implementation of IDisposable
    }
}