using Castle.DynamicProxy;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Abstractions.Utilities;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Go.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Go
{
    public class ProxyFactory : IProxyFactory
    {
        private readonly IRabbitClient _rabbitClient;
        private readonly ITemplateEngine _templateEngine;
        private static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();

        public ProxyFactory(IRabbitClient rabbitClient, ITemplateEngine templateEngine)
        {
            _rabbitClient = rabbitClient;
            _templateEngine = templateEngine;
        }

        #region Implementation of IProxyFactory

        public object CreateProxy(Type interfaceType)
        {
            return ProxyGenerator.CreateInterfaceProxyWithoutTarget(interfaceType, new Type[0], new Interceptor(_rabbitClient, _templateEngine));
        }

        #endregion Implementation of IProxyFactory
    }

    public class Interceptor : IInterceptor
    {
        private readonly IRabbitClient _rabbitClient;
        private readonly ITemplateEngine _templateEngine;

        public Interceptor(IRabbitClient rabbitClient, ITemplateEngine templateEngine)
        {
            _rabbitClient = rabbitClient;
            _templateEngine = templateEngine;
        }

        #region Help Type

        private static class Cache
        {
            #region Field

            private static readonly ConcurrentDictionary<Type, Func<Interceptor, IInvocation, Task>> Caches = new ConcurrentDictionary<Type, Func<Interceptor, IInvocation, Task>>();

            #endregion Field

            public static Func<Interceptor, IInvocation, Task> GetHandler(Type returnType)
            {
                var key = returnType;

                if (Caches.TryGetValue(key, out var handler))
                    return handler;

                var invocationParameterExpression = Expression.Parameter(typeof(IInvocation));
                var instanceParameterExpression = Expression.Parameter(typeof(Interceptor), "instance");

                var callExpression = Expression.Call(instanceParameterExpression, nameof(HandleAsync), new[] { returnType }, invocationParameterExpression);
                handler = Expression.Lambda<Func<Interceptor, IInvocation, Task>>(callExpression, instanceParameterExpression, invocationParameterExpression).Compile();

                Caches.TryAdd(key, handler);

                return handler;
            }
        }

        private struct GoMethodModel
        {
            public IReadOnlyList<string> VariableNames { get; set; }
            public Uri Url { get; set; }
            public int RequestParameterIndex { get; set; }
            public Type RequesType { get; set; }
            public Type ResponseType { get; set; }
            public bool IsAppendQuery { get; set; }
            public Func<IInvocation, object> Handler { get; set; }
            public IDictionary<object, object> Items { get; set; }

            /// <summary>
            /// 如果服务端返回404，则默认返回null而不是抛出异常（这是一个临时的属性）
            /// </summary>
            public bool NotFoundReturnNull { get; set; }
        }

        #endregion Help Type

        #region Implementation of IInterceptor

        public void Intercept(IInvocation invocation)
        {
            var model = GetGoMethodModel(invocation);

            invocation.ReturnValue = model.Handler(invocation);
        }

        #endregion Implementation of IInterceptor

        #region Private Method

        public async Task<T> HandleAsync<T>(IInvocation invocation)
        {
            var value = await InternalHandleAsync(invocation);
            if (value is Task<T> task)
                return await task;
            return (T)value;
        }

        private object Handle(IInvocation invocation)
        {
            return InternalHandleAsync(invocation).GetAwaiter().GetResult();
        }

        private static Dictionary<string, object> AggregateArguments(IInvocation invocation)
        {
            var parameters = invocation.Method.GetParameters();
            if (!parameters.Any())
                return null;

            var arguments = new Dictionary<string, object>(parameters.Length);
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var value = invocation.Arguments[i];
                arguments[parameter.Name] = value;
            }
            return arguments;
        }

        private TemplateRenderContext BuildRequestUrl(string baseUrl, string path, IDictionary<string, object> arguments)
        {
            var url = $"{baseUrl.TrimEnd('/')}{path}";
            return _templateEngine.Render(url, arguments);
        }

        private readonly ConcurrentDictionary<(Type, MethodInfo), GoMethodModel> _goMethodCaches = new ConcurrentDictionary<(Type, MethodInfo), GoMethodModel>();

        private static string GetBaseUrl(MemberInfo proxyType)
        {
            var goClientAttribute = proxyType.GetCustomAttribute<GoClientAttribute>();
            if (goClientAttribute != null && !string.IsNullOrEmpty(goClientAttribute.Url))
                return goClientAttribute.Url;

            var name = proxyType.Name;
            if (name.StartsWith("I"))
            {
                name = name.Substring(1);
            }
            if (name.EndsWith("Service"))
            {
                name = name.Substring(0, name.Length - 7);
            }
            else if (name.EndsWith("Services"))
            {
                name = name.Substring(0, name.Length - 8);
            }

            return "http://" + name;
        }

        private static string GetPath(MemberInfo method)
        {
            var pathProvider = method.GetTypeAttribute<IPathProvider>();
            if (pathProvider != null && !string.IsNullOrEmpty(pathProvider.Path))
                return pathProvider.Path;

            var name = method.Name;
            if (name.EndsWith("Async"))
                name = name.Substring(0, name.Length - 5);

            return name;
        }

        private static Type GetResponseType(MethodInfo method)
        {
            var returnType = method.ReturnType;
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                return returnType.GenericTypeArguments[0];

            return returnType;
        }

        private static int FindRequestParameterIndex(MethodBase method)
        {
            var parameters = method.GetParameters();

            if (!parameters.Any())
                return -1;

            var firstComplexTypeIndex = -1;
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (parameter.GetCustomAttribute<GoBodyAttribute>() != null)
                    return i;
                if (IsComplexType(parameter.ParameterType))
                    firstComplexTypeIndex = i;
            }

            return firstComplexTypeIndex;
        }

        private static bool IsComplexType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.DateTime:
                case TypeCode.Object:
                    return true;

                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.DBNull:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Empty:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.String:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return false;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Type GetProxyType(IInvocation invocation)
        {
            //todo: think of a more reliable way
            var proxyType = invocation.Proxy.GetType();
            var proxyTypeName = proxyType.Name.Substring(0, proxyType.Name.Length - 5);
            return proxyType.GetInterface(proxyTypeName);
        }

        private GoMethodModel GetGoMethodModel(IInvocation invocation)
        {
            var proxyType = GetProxyType(invocation);
            var method = invocation.Method;

            var model = _goMethodCaches.GetOrAdd((proxyType, method), key =>
            {
                var arguments = AggregateArguments(invocation);
                var renderResult = BuildRequestUrl(GetBaseUrl(proxyType), GetPath(method), arguments);

                var returnType = invocation.Method.ReturnType;
                var bodyParameterIndex = FindRequestParameterIndex(method);
                var responseType = GetResponseType(method);
                var methodModel = new GoMethodModel
                {
                    Url = new Uri(renderResult.Result),
                    VariableNames = renderResult.VariableNames,
                    RequestParameterIndex = bodyParameterIndex,
                    RequesType = bodyParameterIndex == -1 ? null : method.GetParameters()[bodyParameterIndex].ParameterType,
                    ResponseType = responseType,
                    IsAppendQuery = arguments != null,
                    Items = GetItems(proxyType, method),
                    NotFoundReturnNull = method.GetCustomAttribute<GoRequestAttribute>()?.NotFoundReturnNull ?? true
                };

                var isTask = typeof(Task).IsAssignableFrom(returnType);

                if (isTask)
                {
                    var handler = Cache.GetHandler(responseType);
                    methodModel.Handler = i => handler(this, i);
                }
                else
                {
                    methodModel.Handler = Handle;
                }

                return methodModel;
            });

            return model;
        }

        private static IDictionary<object, object> GetItems(MemberInfo proxyType, MemberInfo method)
        {
            var context = new ItemProviderContext();
            foreach (var provider in proxyType.GetTypeAttributes<IItemsProvider>().Concat(method.GetTypeAttributes<IItemsProvider>()))
            {
                provider.Collect(context);
            }
            return context.Items;
        }

        private static Uri AppendQueryString(GoMethodModel model, IInvocation invocation)
        {
            if (!model.IsAppendQuery)
                return model.Url;

            bool IsQuery(int index, ParameterInfo parameter)
            {
                if (parameter.GetCustomAttribute<GoParameterAttribute>() != null)
                    return true;
                if (model.VariableNames?.Contains(parameter.Name, StringComparer.OrdinalIgnoreCase) ?? false)
                    return false;
                return index != model.RequestParameterIndex;
            }
            IDictionary<string, string> query = new Dictionary<string, string>();
            var parameters = invocation.Method.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var name = parameter.Name;
                var argument = invocation.Arguments[i];

                if (!IsQuery(i, parameter))
                    continue;

                Append(query, name, argument);
            }

            void Append(IDictionary<string, string> dictionary, string name, object instance)
            {
                if (instance == null)
                {
                    dictionary[name] = StringValues.Empty;
                    return;
                }

                var type = instance.GetType();
                if (!IsComplexType(type))
                {
                    dictionary[name] = instance.ToString();
                }
                else
                {
                    foreach (var property in type.GetProperties())
                    {
                        Append(dictionary, name + "." + property.Name, property.GetValue(instance));
                    }
                }
            }

            return query.Any() ? new Uri(QueryHelpers.AddQueryString(model.Url.ToString(), query)) : model.Url;
        }

        private async Task<object> InternalHandleAsync(IInvocation invocation)
        {
            var model = GetGoMethodModel(invocation);

            var request = model.RequestParameterIndex == -1 ? null : invocation.Arguments[model.RequestParameterIndex];
            var url = AppendQueryString(model, invocation);

            var requestMessage = new RabbitRequestMessage(model.RequesType, model.ResponseType, url, request, null, new Dictionary<object, object>(model.Items));

            try
            {
                var response = await _rabbitClient.SendAsync(requestMessage);
                return response.Body;
            }
            catch (RabbitClientException e) when (model.NotFoundReturnNull && e.StatusCode == 404)//todo:临时实现，考虑使用拦截器实现
            {
                return null;
            }
        }

        #endregion Private Method
    }
}