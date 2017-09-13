using Castle.DynamicProxy;
using Rabbit.Cloud.Facade.Features;
using Rabbit.Cloud.Facade.Internal;
using RC.Discovery.Client.Abstractions;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade.Middlewares
{
    public class FacadeMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly IRequestMessageBuilderProvider _requestMessageBuilderProvider;

        public FacadeMiddleware(RabbitRequestDelegate next, IRequestMessageBuilderProvider requestMessageBuilderProvider)
        {
            _next = next;
            _requestMessageBuilderProvider = requestMessageBuilderProvider;
        }

        public async Task Invoke(RabbitContext context)
        {
            var invocationFeature = context.Features.Get<IInvocationFeature>();
            var invocation = invocationFeature.Invocation;

            SetRequest(context, invocation);

            await _next(context);
        }

        #region Private Method

        private void SetRequest(RabbitContext context, IInvocation invocation)
        {
            _requestMessageBuilderProvider.Build(invocation, context);
        }

        #endregion Private Method
    }
}