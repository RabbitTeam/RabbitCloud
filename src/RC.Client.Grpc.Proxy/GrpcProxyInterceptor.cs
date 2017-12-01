using Castle.DynamicProxy;
using Grpc.Core;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Abstractions.Serialization;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Features;
using Rabbit.Cloud.Client.Proxy;
using Rabbit.Cloud.Grpc.Fluent;
using Rabbit.Cloud.Grpc.Fluent.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Grpc.Proxy
{
    public class GrpcProxyInterceptor : RabbitProxyInterceptor
    {
        private readonly IEnumerable<ISerializer> _serializers;
        private static readonly Type[] IgnoreGenericTypes = { typeof(AsyncServerStreamingCall<>), typeof(AsyncDuplexStreamingCall<,>) };

        public GrpcProxyInterceptor(RabbitRequestDelegate invoker, IOptions<GrpcOptions> options) : base(invoker)
        {
            _serializers = options.Value.Serializers;
        }

        #region Overrides of RabbitProxyInterceptor

        protected override IRabbitContext CreateRabbitContext(IInvocation invocation)
        {
            var proxyType = invocation.Proxy.GetType();
            var proxyTypeName = proxyType.Name.Substring(0, proxyType.Name.Length - 5);
            proxyType = proxyType.GetInterface(proxyTypeName);

            //todo: think of a more reliable way
            var fullServiceName = FluentUtilities.GetFullServiceName(proxyType, invocation.Method);

            var context = new GrpcRabbitContext();

            context.Request.Url = new ServiceUrl
            {
                Scheme = "grpc",
                Path = fullServiceName
            };

            var parameters = invocation.Method.GetParameters();

            var dictionary = new Dictionary<string, object>();
            var index = 0;
            foreach (var parameter in parameters)
            {
                dictionary[parameter.Name] = invocation.Arguments[index];
                index++;
            }

            var requestModel = FluentUtilities.GetRequestModel(dictionary, _serializers);

            context.Request.Request = requestModel;

            return context;
        }

        protected override async Task<object> ConvertReturnValue(IInvocation invocation, IRabbitContext rabbitContext)
        {
            var response = ((GrpcRabbitContext)rabbitContext).Response.Response;
            if (response == null)
                return null;

            var returnType = invocation.Method.ReturnType;

            if (returnType == typeof(void))
                return null;

            var responseType = response.GetType();
            if (IgnoreGenericTypes.Contains(responseType.GetGenericTypeDefinition()))
                return response;

            if (!typeof(Task).IsAssignableFrom(returnType))
                return response;

            var responseAsyncPropertyAccessor = Cache.GetResponseAsyncAccessor(responseType);
            var responseAsync = responseAsyncPropertyAccessor(response);

            if (!returnType.IsGenericType)
                return responseAsync;

            await responseAsync;

            var taskResultAccessor = Cache.GetTaskResultAccessor(responseAsync);
            return taskResultAccessor(responseAsync);
        }

        #endregion Overrides of RabbitProxyInterceptor

        #region Help Type

        private static class Cache
        {
            #region Field

            private static readonly ConcurrentDictionary<object, Lazy<object>> Caches = new ConcurrentDictionary<object, Lazy<object>>();

            #endregion Field

            public static Func<object, Task> GetResponseAsyncAccessor(Type type)
            {
                var key = ("ResponseAsyncAccessor", type);

                return GetCache(key, () =>
                {
                    var parameterExpression = Expression.Parameter(typeof(object));
                    var convertExpression = Expression.Convert(parameterExpression, type);

                    var responseAsyncPropertyExpression = Expression.Property(convertExpression, nameof(AsyncUnaryCall<object>.ResponseAsync));
                    return Expression.Lambda<Func<object, Task>>(responseAsyncPropertyExpression, parameterExpression).Compile();
                });
            }

            public static Func<Task, object> GetTaskResultAccessor(Task task)
            {
                var type = task.GetType();
                if (!type.IsGenericType)
                    throw new ArgumentException("type is not Task<>.", nameof(type));

                var key = ("TaskResultAccessor", type);

                return GetCache(key, () =>
                {
                    var parameterExpression = Expression.Parameter(typeof(object));

                    var getAwaiterMethodInfo = type.GetMethod(nameof(Task<object>.GetAwaiter));

                    var callExpression = Expression.Call(Expression.Call(Expression.Convert(parameterExpression, type), getAwaiterMethodInfo), nameof(TaskAwaiter.GetResult), null);

                    return Expression.Lambda<Func<Task, object>>(callExpression, parameterExpression).Compile();
                });
            }

            #region Private Method

            private static T GetCache<T>(object key, Func<T> factory)
            {
                var item = Caches.GetOrAdd(key, new Lazy<object>(() => factory()));
                return (T)item.Value;
            }

            #endregion Private Method
        }

        #endregion Help Type
    }
}