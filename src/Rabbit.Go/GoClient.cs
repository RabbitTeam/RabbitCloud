using Rabbit.Go.Abstractions;
using System;
using System.Threading.Tasks;

namespace Rabbit.Go
{
    public class GoClient : IGoClient
    {
        private readonly Func<RequestContext, IGoRequestInvoker> _invokerFactory;

        public GoClient(Func<RequestContext, IGoRequestInvoker> invokerFactory)
        {
            _invokerFactory = invokerFactory;
        }

        #region Implementation of IGoClient

        public async Task RequestAsync(RequestContext context)
        {
            await _invokerFactory(context).InvokeAsync();
        }

        #endregion Implementation of IGoClient
    }
}