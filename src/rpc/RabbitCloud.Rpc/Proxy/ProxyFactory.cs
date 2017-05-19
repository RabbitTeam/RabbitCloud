using Castle.DynamicProxy;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Proxy;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Proxy
{
    public class ProxyFactory : IProxyFactory
    {
        private readonly IRequestIdGenerator _requestIdGenerator;
        private readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();

        public ProxyFactory(IRequestIdGenerator requestIdGenerator)
        {
            _requestIdGenerator = requestIdGenerator;
        }

        #region Implementation of IProxyFactory

        public T GetProxy<T>(ICaller caller)
        {
            return (T)_proxyGenerator.CreateInterfaceProxyWithoutTarget(typeof(T), new[] { typeof(T) }, new CallerInterceptor(caller, _requestIdGenerator));
        }

        #endregion Implementation of IProxyFactory

        private class CallerInterceptor : IInterceptor
        {
            private static readonly MethodInfo HandleAsyncMethodInfo = typeof(CallerInterceptor).GetMethod(nameof(HandleAsync), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
            private readonly ICaller _caller;
            private readonly IRequestIdGenerator _requestIdGenerator;

            public CallerInterceptor(ICaller caller, IRequestIdGenerator requestIdGenerator)
            {
                _caller = caller;
                _requestIdGenerator = requestIdGenerator;
            }

            #region Implementation of IInterceptor

            public void Intercept(IInvocation invocation)
            {
                var request = GetRequest(invocation);

                var returnType = invocation.Method.ReturnType;
                var isTask = typeof(Task).IsAssignableFrom(returnType);

                if (isTask)
                {
                    returnType = returnType.GenericTypeArguments.FirstOrDefault() ?? typeof(object);
                    invocation.ReturnValue = HandleAsyncMethodInfo.MakeGenericMethod(returnType)
                        .Invoke(this, new object[] { request });
                }
                else
                {
                    invocation.ReturnValue = Handle(request);
                }
            }

            #endregion Implementation of IInterceptor

            private IRequest GetRequest(IInvocation invocation)
            {
                return new Request
                {
                    Arguments = invocation.Arguments,
                    MethodDescriptor = new MethodDescriptor(invocation.Method),
                    RequestId = _requestIdGenerator.GetRequestId()
                };
            }

            private async Task<T> HandleAsync<T>(IRequest request)
            {
                var response = await _caller.CallAsync(request);

                return (T)response.Value;
            }

            private object Handle(IRequest request)
            {
                return Task.Run(async () =>
                {
                    var response = await _caller.CallAsync(request);
                    return response.Value;
                }).Result;
            }
        }
    }
}