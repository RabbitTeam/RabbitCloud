using Castle.DynamicProxy;
using Grpc.Core;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Abstractions.Serialization;
using Rabbit.Cloud.Abstractions.Utilities;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Features;
using Rabbit.Cloud.Client.Proxy;
using Rabbit.Cloud.Grpc.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Grpc.Proxy
{
    public class GrpcProxyInterceptor : RabbitProxyInterceptor
    {
        private readonly IEnumerable<ISerializer> _serializers;
        private static readonly Type[] IgnoreGenericTypes = { typeof(AsyncServerStreamingCall<>), typeof(AsyncDuplexStreamingCall<,>) };

        public GrpcProxyInterceptor(RabbitRequestDelegate invoker, IOptions<RabbitCloudOptions> options) : base(invoker)
        {
            _serializers = options.Value.Serializers;
        }

        #region Overrides of RabbitProxyInterceptor

        protected override IRabbitContext CreateRabbitContext(IInvocation invocation)
        {
            var context = new GrpcRabbitContext();

            context.Request.Url = GetOrCreateServiceUrl(invocation);

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

            var responseAsync = FluentUtilities.WrapperCallResuleToTask(response);
            await responseAsync;

            if (!returnType.IsGenericType)
                return responseAsync;

            var result = Cache.GetTaskResult(responseAsync);
            return result;
        }

        #endregion Overrides of RabbitProxyInterceptor

        #region Private Method

        private readonly ConcurrentDictionary<Type, ServiceUrl> _serviceUrlCaches = new ConcurrentDictionary<Type, ServiceUrl>();

        public static ServiceUrl CreateServiceUrl(IInvocation invocation)
        {
            var proxyType = invocation.Proxy.GetType();
            var proxyTypeName = proxyType.Name.Substring(0, proxyType.Name.Length - 5);
            proxyType = proxyType.GetInterface(proxyTypeName);

            //todo: think of a more reliable way
            var fullServiceName = FluentUtilities.GetFullServiceName(proxyType, invocation.Method);
            var clientDefinitionProvider = proxyType.GetTypeAttribute<IClientDefinitionProvider>();

            var host = clientDefinitionProvider.Host;
            var port = 0;
            if (host.Contains(":"))
            {
                var temp = host.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                host = temp[0];
                if (temp.Length == 2)
                    int.TryParse(temp[1], out port);
            }

            return new ServiceUrl
            {
                Host = host,
                Path = fullServiceName,
                Port = port,
                Scheme = clientDefinitionProvider.Protocol
            };
        }

        private ServiceUrl GetOrCreateServiceUrl(IInvocation invocation)
        {
            var type = invocation.Proxy.GetType();
            if (_serviceUrlCaches.TryGetValue(type, out var url))
                return url;

            url = CreateServiceUrl(invocation);
            _serviceUrlCaches.TryAdd(type, url);

            return url;
        }

        #endregion Private Method

        #region Help Type

        private static class Cache
        {
            #region Field

            private static readonly ConcurrentDictionary<Type, Func<Task, object>> Caches = new ConcurrentDictionary<Type, Func<Task, object>>();

            #endregion Field

            public static object GetTaskResult(Task task)
            {
                var type = task.GetType();
                if (!type.IsGenericType)
                    throw new ArgumentException("type is not Task<>.", nameof(type));

                if (Caches.TryGetValue(type, out var accesser))
                    return accesser(task);

                var parameterExpression = Expression.Parameter(typeof(Task));
                var convertExpression = Expression.Convert(parameterExpression, task.GetType());

                accesser = Expression.Lambda<Func<Task, object>>(Expression.Property(convertExpression, "Result"), parameterExpression).Compile();
                Caches.TryAdd(type, accesser);

                return accesser(task);
            }
        }

        #endregion Help Type
    }
}