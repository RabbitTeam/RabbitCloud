using RabbitCloud.Abstractions;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Internal
{
    public class DefaultProvider : Provider
    {
        private readonly Func<object> _getProxyInstance;

        public DefaultProvider(Func<object> getProxyInstance, Url url, Type type) : base(url, type)
        {
            _getProxyInstance = getProxyInstance;
        }

        #region Overrides of Provider

        /// <summary>
        /// 针对RPC请求执行调用。
        /// </summary>
        /// <param name="request">RPC请求。</param>
        /// <returns>RPC响应结果。</returns>
        protected override async Task<IResponse> Invoke(IRequest request)
        {
            var response = new DefaultResponse();
            var method = GetMethod(request);

            try
            {
                var result = method.Invoke(_getProxyInstance(), request.Arguments);

                var resultTask = result as Task;
                if (resultTask != null)
                    result = await GetResultByTask(resultTask);

                response.Result = result;
            }
            catch (Exception exception)
            {
                response.Exception = exception.InnerException ?? exception;
            }
            return response;
        }

        #endregion Overrides of Provider

        #region Private Method

        private static async Task<object> GetResultByTask(Task task)
        {
            await task;
            var type = task.GetType();
            return type.GenericTypeArguments.Any() ? type.GetProperty("Result").GetValue(task) : null;
        }

        #endregion Private Method
    }
}