using RabbitCloud.Abstractions.Utilities;
using RabbitCloud.Rpc.Abstractions;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc
{
    public class TypeCaller : INamedCaller
    {
        private readonly Type _serviceType;
        private readonly Func<object> _factory;

        public TypeCaller(object instance) : this(instance.GetType(), () => instance)
        {
        }

        public TypeCaller(Type serviceType, Func<object> factory)
        {
            _serviceType = serviceType;
            _factory = factory;
            Name = _serviceType.Name;
        }

        #region Implementation of ICaller

        public async Task<IResponse> CallAsync(IRequest request)
        {
            var method = MatchMethod(request.MethodKey);
            var response = new Response(request);
            try
            {
                response.Value = await ExecutionAsync(method, _factory(), request.Arguments);
            }
            catch (Exception exception)
            {
                response.Exception = exception;
            }
            return response;
        }

        #endregion Implementation of ICaller

        #region Implementation of INamedCaller

        public string Name { get; }

        #endregion Implementation of INamedCaller

        #region Private Method

        private static async Task<object> ExecutionAsync(MethodBase method, object instance, object[] arguments)
        {
            var result = method.Invoke(instance, arguments);

            var task = result as Task;
            if (task == null)
                return result;

            await task;

            var resultType = result.GetType();
            result = resultType.GenericTypeArguments.Any() ? resultType.GetRuntimeProperty("Result").GetValue(task) : null;
            return result;
        }

        private MethodInfo MatchMethod(MethodKey key)
        {
            return _serviceType.GetMethods()
                .SingleOrDefault(method => ReflectUtil.GetMethodDesc(method) ==
                                           ReflectUtil.GetMethodDesc(key.Name, key.ParamtersDesc));
        }

        #endregion Private Method
    }
}