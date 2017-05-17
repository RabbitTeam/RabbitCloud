using RabbitCloud.Rpc.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc
{
    public class MethodCaller : INamedCaller
    {
        private readonly Func<object> _instanceFactory;
        private readonly MethodInfo _methodInfo;
        private readonly Func<string, bool> _nameAvailableFunc;

        public MethodCaller(object instance, MethodInfo methodInfo, Func<string, bool> nameAvailableFunc = null) : this(() => instance, methodInfo, nameAvailableFunc)
        {
        }

        public MethodCaller(Func<object> instanceFactory, MethodInfo methodInfo, Func<string, bool> nameAvailableFunc = null)
        {
            _instanceFactory = instanceFactory;
            _methodInfo = methodInfo;
            _nameAvailableFunc = nameAvailableFunc;
            Name = GetCallerName(methodInfo);
        }

        #region Implementation of ICaller

        public async Task<IResponse> CallAsync(IRequest request)
        {
            var response = new Response(request);
            object result = null;
            try
            {
                result = _methodInfo.Invoke(_instanceFactory(), request.Arguments);
            }
            catch (Exception exception)
            {
                response.Exception = exception;
            }

            var task = result as Task;
            if (task != null)
            {
                await task;

                var resultType = result.GetType();
                result = resultType.GenericTypeArguments.Any() ? resultType.GetRuntimeProperty("Result").GetValue(task) : null;
            }
            response.Value = result;

            return response;
        }

        #endregion Implementation of ICaller

        #region Implementation of INamedCaller

        public string Name { get; }

        #endregion Implementation of INamedCaller

        private string GetCallerName(MethodBase methodInfo)
        {
            return GetCallerNames(methodInfo).First(i => _nameAvailableFunc == null || _nameAvailableFunc(i));
        }

        private static IEnumerable<string> GetCallerNames(MethodBase methodInfo)
        {
            var name = methodInfo.Name;
            yield return name;

            name = methodInfo.Name;
            foreach (var parameterInfo in methodInfo.GetParameters())
            {
                name += "_" + parameterInfo.Name;
            }
            yield return name;

            name = methodInfo.Name;
            foreach (var parameterInfo in methodInfo.GetParameters())
            {
                name += "_" + parameterInfo.ParameterType.Name;
            }
            yield return name;

            name = methodInfo.Name;
            foreach (var parameterInfo in methodInfo.GetParameters())
            {
                name += "_" + parameterInfo.ParameterType.FullName;
            }
            yield return name;
        }
    }
}