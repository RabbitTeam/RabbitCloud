using Castle.DynamicProxy;
using Rabbit.Cloud.Client.Abstractions;
using System;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Proxy
{
    public class SimpleRabbitProxyInterceptor : RabbitProxyInterceptor
    {
        private readonly Func<IInvocation, IRabbitContext> _rabbitContextFactory;
        private readonly Func<IInvocation, IRabbitContext, Task<object>> _returnValueConver;

        public SimpleRabbitProxyInterceptor(RabbitRequestDelegate invoker, Func<IInvocation, IRabbitContext> rabbitContextFactory, Func<IInvocation, IRabbitContext, Task<object>> returnValueConver) : base(invoker)
        {
            _rabbitContextFactory = rabbitContextFactory;
            _returnValueConver = returnValueConver;
        }

        #region Overrides of RabbitProxyInterceptor

        protected override IRabbitContext CreateRabbitContext(IInvocation invocation)
        {
            return _rabbitContextFactory(invocation);
        }

        protected override Task<object> ConvertReturnValue(IInvocation invocation, IRabbitContext rabbitContext)
        {
            return _returnValueConver(invocation, rabbitContext);
        }

        #endregion Overrides of RabbitProxyInterceptor
    }
}