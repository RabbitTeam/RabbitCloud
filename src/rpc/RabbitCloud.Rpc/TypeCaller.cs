using Microsoft.Extensions.Logging;
using RabbitCloud.Abstractions.Exceptions;
using RabbitCloud.Abstractions.Logging;
using RabbitCloud.Abstractions.Utilities;
using RabbitCloud.Rpc.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc
{
    public class TypeCaller : INamedCaller
    {
        private readonly Type _serviceType;
        private readonly Func<object> _factory;
        private readonly ILogger<TypeCaller> _logger;
        private static readonly Dictionary<MethodDescriptor, MethodInfo> MethodCache = new Dictionary<MethodDescriptor, MethodInfo>();

        public TypeCaller(object instance, ILogger<TypeCaller> logger = null) : this(instance.GetType(), () => instance, logger)
        {
        }

        public TypeCaller(Type serviceType, Func<object> factory, ILogger<TypeCaller> logger = null)
        {
            _serviceType = serviceType;
            _factory = factory;
            _logger = logger ?? NullLogger<TypeCaller>.Instance;
            Name = _serviceType.Name;
        }

        #region Implementation of ICaller

        public bool IsAvailable { get; } = true;

        public async Task<IResponse> CallAsync(IRequest request)
        {
            var method = Lookup(request.MethodDescriptor);

            var response = new Response(request);
            if (method == null)
            {
                response.Exception = new RabbitServiceException($"Service method not exist: {request.MethodDescriptor.InterfaceName}.{request.MethodDescriptor.MethodName}({request.MethodDescriptor.ParamtersSignature})");
                return response;
            }

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

        private async Task<object> ExecutionAsync(MethodBase method, object instance, object[] arguments)
        {
            try
            {
                var result = method.Invoke(instance, arguments);

                var task = result as Task;
                if (task == null)
                    return result;

                await task;

                var resultType = result.GetType();
                result = resultType.GenericTypeArguments.Any()
                    ? resultType.GetRuntimeProperty("Result").GetValue(task)
                    : null;
                return result;
            }
            catch (Exception e) when (e.InnerException != null)
            {
                var exception = e.InnerException;
                _logger.LogError(0, exception, "Exception caught when method invoke: " + exception);
                throw new RabbitBusinessException("provider call process error", exception);
            }
            catch (Exception e)
            {
                throw new RabbitBusinessException("provider call process error", e);
            }
        }

        private MethodInfo Lookup(MethodDescriptor methodDescriptor)
        {
            if (MethodCache.TryGetValue(methodDescriptor, out MethodInfo methodInfo))
                return methodInfo;

            methodInfo = _serviceType.GetMethods()
                .SingleOrDefault(method => ReflectUtil.GetMethodDesc(method) ==
                                           ReflectUtil.GetMethodDesc(methodDescriptor.MethodName, methodDescriptor.ParamtersSignature));

            MethodCache[methodDescriptor] = methodInfo;

            return methodInfo;
        }

        #endregion Private Method
    }
}