using Castle.DynamicProxy;
using RabbitCloud.Abstractions.Exceptions;
using RabbitCloud.Abstractions.Exceptions.Extensions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Proxy;
using System;
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
                var request = new Request
                {
                    Arguments = invocation.Arguments,
                    MethodDescriptor = new MethodDescriptor(invocation.Method),
                    RequestId = _requestIdGenerator.GetRequestId()
                };
                var requestOptions = invocation.Arguments.FirstOrDefault(i => i is RequestOptions);

                if (requestOptions != null)
                    request.SetRequestOptions((RequestOptions)requestOptions);

                return request;
            }

            private async Task<T> HandleAsync<T>(IRequest request)
            {
                var response = await _caller.CallAsync(request);

                return (T)ReturnValue(response);
            }

            private object Handle(IRequest request)
            {
                return Task.Run(async () =>
                {
                    var response = await _caller.CallAsync(request);
                    return ReturnValue(response);
                }).Result;
            }

            private static object ReturnValue(IResponse response)
            {
                try
                {
                    return response.GetValue();
                }
                catch (Exception exception)
                {
                    if (!exception.IsBusinessException())
                        throw;
                    var e = exception.InnerException;
                    if (e != null)
                        throw e;
                    throw new RabbitServiceException("biz exception cause is null");
                }
            }
        }
    }
}