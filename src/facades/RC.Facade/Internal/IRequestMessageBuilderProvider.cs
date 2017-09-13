using Castle.DynamicProxy;
using RC.Discovery.Client.Abstractions;
using System.Collections.Generic;

namespace Rabbit.Cloud.Facade.Internal
{
    public interface IRequestMessageBuilderProvider
    {
        void Build(IInvocation invocation, RabbitContext rabbitContext);
    }

    public class RequestMessageBuilderProvider : IRequestMessageBuilderProvider
    {
        private readonly IEnumerable<IRequestMessageBuilder> _requestMessageBuilders;

        public RequestMessageBuilderProvider(IEnumerable<IRequestMessageBuilder> requestMessageBuilders)
        {
            _requestMessageBuilders = requestMessageBuilders;
        }

        #region Implementation of IRequestMessageBuilderProvider

        public void Build(IInvocation invocation, RabbitContext rabbitContext)
        {
            var context = new RequestMessageBuilderContext(invocation.Method, invocation.Arguments, rabbitContext);

            foreach (var requestMessageBuilder in _requestMessageBuilders)
            {
                if (context.Canceled)
                    break;
                requestMessageBuilder.Build(context);
            }
        }

        #endregion Implementation of IRequestMessageBuilderProvider
    }
}