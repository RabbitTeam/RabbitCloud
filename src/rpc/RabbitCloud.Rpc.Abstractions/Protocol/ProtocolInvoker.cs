using RabbitCloud.Abstractions;
using System;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Protocol
{
    public abstract class ProtocolInvoker : IInvoker
    {
        protected ProtocolInvoker(Url url)
        {
            Url = url;
        }

        #region Implementation of IInvoker

        public Url Url { get; }

        public async Task<IResult> Invoke(IInvocation invocation)
        {
            try
            {
                return await DoInvoke(invocation);
            }
            catch (Exception exception)
            {
                return new Result(exception);
            }
        }

        #endregion Implementation of IInvoker

        protected abstract Task<IResult> DoInvoke(IInvocation invocation);
    }
}