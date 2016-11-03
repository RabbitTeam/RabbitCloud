using RabbitCloud.Abstractions;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Proxy
{
    public abstract class ProxyInvoker : IInvoker
    {
        private readonly Func<object> _getInstance;

        protected ProxyInvoker(Url url, Func<object> getInstance)
        {
            Url = url;
            _getInstance = getInstance;
        }

        #region Implementation of IInvoker

        /// <summary>
        /// 进行调用。
        /// </summary>
        /// <param name="invocation">调用信息。</param>
        /// <returns>返回结果。</returns>
        public async Task<IResult> Invoke(IInvocation invocation)
        {
            try
            {
                var instance = _getInstance();

                var result = await DoInvoke(instance, invocation.MethodName, invocation.ParameterTypes, invocation.Arguments);

                result = await GetValue(result);

                return new RpcResult(result);
            }
            catch (Exception exception)
            {
                return new RpcResult(exception);
            }
        }

        #endregion Implementation of IInvoker

        #region Protected Method

        /// <summary>
        /// 进行调用。
        /// </summary>
        /// <param name="proxy">代理实例。</param>
        /// <param name="methodName">方法名称。</param>
        /// <param name="parameterTypes">参数类型。</param>
        /// <param name="arguments">方法参数。</param>
        /// <returns>返回值。</returns>
        protected abstract Task<object> DoInvoke(object proxy, string methodName, Type[] parameterTypes, object[] arguments);

        #endregion Protected Method

        #region Implementation of INode

        /// <summary>
        /// 节点Url。
        /// </summary>
        public Url Url { get; }

        /// <summary>
        /// 是否可用。
        /// </summary>
        public bool IsAvailable { get; } = true;

        #endregion Implementation of INode

        #region Implementation of IDisposable

        /// <summary>
        /// 执行与释放或重置非托管资源关联的应用程序定义的任务。
        /// </summary>
        public void Dispose()
        {
        }

        #endregion Implementation of IDisposable

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