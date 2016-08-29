using RabbitCloud.Abstractions;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Proxy
{
    public abstract class ProxyInvoker : IInvoker
    {
        private readonly Func<object> _getInstance;

        protected ProxyInvoker(Id id, Func<object> getInstance)
        {
            _getInstance = getInstance;
            Id = id;
        }

        #region Implementation of IInvoker

        public Id Id { get; }

        public async Task<IResult> Invoke(IInvocation invocation)
        {
            try
            {
                var instance = _getInstance();

                var result = await DoInvoke(instance, invocation.MethodName, invocation.ParameterTypes, invocation.Arguments);

                result = await GetValue(result);

                return new Result(result);
            }
            catch (Exception exception)
            {
                return new Result(exception);
            }
        }

        #endregion Implementation of IInvoker

        protected abstract Task<object> DoInvoke(object proxy, string methodName, Type[] parameterTypes, object[] arguments);

        #region Private Method

        private static async Task<object> GetValue(object value)
        {
            var task = value as Task;
            if (task == null)
                return value;

            await task;

            var taskType = task.GetType().GetTypeInfo();

            return taskType.IsGenericType ? taskType.GetProperty("Result").GetValue(task) : null;
        }

        #endregion Private Method
    }
}