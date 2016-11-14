using Castle.DynamicProxy;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Proxy.Castle
{
    /// <summary>
    /// 调用者拦截器。
    /// </summary>
    internal class InvokerInterceptor : IInterceptor
    {
        private readonly InvocationDelegate _invocationHandler;

        public InvokerInterceptor(InvocationDelegate invocationHandler)
        {
            _invocationHandler = invocationHandler;
        }

        #region Implementation of IInterceptor

        public void Intercept(IInvocation invocation)
        {
            var returnType = invocation.Method.ReturnType.GetTypeInfo();
            var isTask = typeof(Task).GetTypeInfo().IsAssignableFrom(returnType);

            var invokeTask = _invocationHandler(invocation.Proxy, invocation.Method, invocation.Arguments);

            var result = invokeTask.GetAwaiter().GetResult();

            if (!isTask)
            {
                invocation.ReturnValue = result;
            }
            else
            {
                var value = result;
                if (returnType.IsGenericType)
                {
                    var taskGenericType = returnType.GenericTypeArguments[0];
                    var fromResult = typeof(Task).GetRuntimeMethods().First(i => i.Name == "FromResult");
                    invocation.ReturnValue = fromResult.MakeGenericMethod(taskGenericType).Invoke(null, new[] { value });
                }
                else
                {
                    invocation.ReturnValue = Task.CompletedTask;
                }
            }
        }

        #endregion Implementation of IInterceptor
    }
}