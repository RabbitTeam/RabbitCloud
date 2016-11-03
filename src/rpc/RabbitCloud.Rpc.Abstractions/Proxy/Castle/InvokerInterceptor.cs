using Castle.DynamicProxy;
using System;
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
        private readonly IInvoker _invoker;

        public InvokerInterceptor(IInvoker invoker)
        {
            _invoker = invoker;
        }

        #region Implementation of IInterceptor

        public void Intercept(global::Castle.DynamicProxy.IInvocation invocation)
        {
            var returnType = invocation.Method.ReturnType.GetTypeInfo();
            var isTask = typeof(Task).GetTypeInfo().IsAssignableFrom(returnType);

            var invokeTask = isTask ? _invoker.Invoke(RpcInvocation.Create(invocation.Method, invocation.Arguments)) : Task.Run(() => _invoker.Invoke(RpcInvocation.Create(invocation.Method, invocation.Arguments)));

            if (!isTask)
            {
                invocation.ReturnValue = invokeTask.Result;
            }
            else
            {
                var result = invokeTask.GetAwaiter().GetResult();
                var value = result.Recreate();
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
            /*var result = _invoker.Invoke(Invocation.Create(invocation.Method, invocation.Arguments)).Result;
            var value = result.Recreate();

            var returnType = invocation.Method.ReturnType.GetTypeInfo();
            if (!typeof(Task).GetTypeInfo().IsAssignableFrom(returnType))
                invocation.ReturnValue = value;
            else
            {
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
            }*/
            try
            {
                invocation.Proceed();
            }
            catch (NotImplementedException)
            {
            }
        }

        #endregion Implementation of IInterceptor
    }
}