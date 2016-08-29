using RabbitCloud.Abstractions;
using System;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Protocol
{
    public abstract class ProtocolInvoker : IInvoker
    {
        protected ProtocolInvoker(Id id)
        {
            Id = id;
        }

        #region Implementation of IInvoker

        public Id Id { get; }

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