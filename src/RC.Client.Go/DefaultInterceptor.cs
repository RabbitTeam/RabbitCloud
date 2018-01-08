using Castle.DynamicProxy;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Go.Abstractions;
using Rabbit.Cloud.Client.Go.ApplicationModels;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Go
{
    public class DefaultInterceptor : IInterceptor
    {
        private readonly IRabbitClient _rabbitClient;
        private readonly ITemplateEngine _templateEngine;
        private readonly ApplicationModel _applicationModel;

        public DefaultInterceptor(IRabbitClient rabbitClient, ITemplateEngine templateEngine, ApplicationModel applicationModel)
        {
            _rabbitClient = rabbitClient;
            _templateEngine = templateEngine;
            _applicationModel = applicationModel;
        }

        #region Implementation of IInterceptor

        public void Intercept(IInvocation invocation)
        {
            var entry = GetRequestEntry(GetProxyType(invocation), invocation.Method);
            var goRequestContext = new GoRequestContext(entry, invocation);

            invocation.ReturnValue = entry.Handler(goRequestContext);
        }

        #endregion Implementation of IInterceptor

        #region Private Method

        private async Task<T> HandleAsync<T>(GoRequestContext goRequestContext)
        {
            var value = await InternalHandleAsync(goRequestContext);
            if (value is Task<T> task)
                return await task;
            return (T)value;
        }

        private object Handle(GoRequestContext goRequestContext)
        {
            return InternalHandleAsync(goRequestContext).GetAwaiter().GetResult();
        }

        private struct GoParameterContext
        {
            public GoParameterContext(ParameterModel parameterModel, object argument)
            {
                ParameterModel = parameterModel;
                Argument = argument;
                OnlyOneParameter = parameterModel.Request.Parameters.Count == 1;
                Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            public ParameterModel ParameterModel { get; }
            public IDictionary<string, string> Values { get; }
            public bool OnlyOneParameter { get; }
            public object Argument { get; }

            public void AppendValue(string name, string value)
            {
                Values[name] = value;
            }
        }

        private static void BuildParameters(GoRequestContext goRequestContext)
        {
            var arguments = goRequestContext.Arguments;
            var parameterModels = goRequestContext.RequestEntry.RequestModel.Parameters;

            foreach (var parameterModel in parameterModels)
            {
                var key = parameterModel.ParameterName;
                var argument = arguments[parameterModel.ParameterInfo.Name];

                IDictionary<string, string> GetSimpleBuild()
                {
                    var context = new GoParameterContext(parameterModel, argument);
                    return GetSimpleValue(context);
                }
                switch (parameterModel.Target)
                {
                    case ParameterTarget.Query:
                        goRequestContext.AppendQuery(GetSimpleBuild());
                        break;

                    case ParameterTarget.Header:
                        goRequestContext.AppendHeaders(GetSimpleBuild());
                        break;

                    case ParameterTarget.Items:
                        goRequestContext.AppendItems(key, argument);
                        break;

                    case ParameterTarget.Path:
                        goRequestContext.AppendPathVariable(GetSimpleBuild());
                        break;

                    case ParameterTarget.Body:
                        goRequestContext.SetBody(argument);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static IDictionary<string, string> GetSimpleValue(GoParameterContext context)
        {
            SimpleBuild(context.ParameterModel.ParameterName, context.Argument, context);
            return context.Values;
        }

        private static void SimpleBuild(string name, object value, GoParameterContext context)
        {
            if (value == null)
                return;

            var type = value.GetType();
            var code = Type.GetTypeCode(type);

            bool IsEnumerable()
            {
                return value is IEnumerable;
            }

            if (code == TypeCode.Object)
            {
                if (IsEnumerable())
                {
                    BuildEnumerable(name, (IEnumerable)value, context);
                }
                else
                {
                    BuildModel(context.OnlyOneParameter ? string.Empty : name, value, context);
                }
            }
            else
            {
                context.AppendValue(name, value.ToString());
            }
        }

        private static void BuildEnumerable(string name, IEnumerable enumerable, GoParameterContext context)
        {
            var enumerator = enumerable.GetEnumerator();
            var index = 0;
            while (enumerator.MoveNext())
            {
                SimpleBuild($"{name}[{index}]", enumerator.Current, context);
                index++;
            }
        }

        private static void BuildModel(string name, object model, GoParameterContext context)
        {
            var type = model.GetType();
            foreach (var property in type.GetProperties())
            {
                var propertyValue = property.GetValue(model);

                var chidName = string.IsNullOrEmpty(name) ? property.Name : name + "." + property.Name;

                SimpleBuild(chidName, propertyValue, context);
            }
        }

        private async Task<object> InternalHandleAsync(GoRequestContext goRequestContext)
        {
            var requestEntry = goRequestContext.RequestEntry;
            var requestModel = requestEntry.RequestModel;

            var requestType = requestModel.RequesType;
            var responseType = requestModel.ResponseType;

            BuildParameters(goRequestContext);

            var renderResult = _templateEngine.Render(goRequestContext.RequestUrl, goRequestContext.PathVariables);

            var requestUrl = renderResult?.Result ?? goRequestContext.RequestUrl;
            if (goRequestContext.Query != null)
                requestUrl = QueryHelpers.AddQueryString(requestUrl, goRequestContext.Query.ToDictionary(i => i.Key, i => i.Value.ToString()));

            var requestMessage = new RabbitRequestMessage(requestType, responseType, new Uri(requestUrl), goRequestContext.Body, goRequestContext.Headers, goRequestContext.Items);

            return await SendAsync(goRequestContext, requestMessage);
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

        private readonly ConcurrentDictionary<(Type, MethodInfo), RequestEntry> _requestEntries = new ConcurrentDictionary<(Type, MethodInfo), RequestEntry>();

        private static Type GetProxyType(IInvocation invocation)
        {
            //todo: think of a more reliable way
            var proxyType = invocation.Proxy.GetType();
            var proxyTypeName = proxyType.Name.Substring(0, proxyType.Name.Length - 5);
            return proxyType.GetInterface(proxyTypeName);
        }

        private static IDictionary<string, StringValues> GetHeaders(RequestModel requestModel)
        {
            var providers = GetRequestAttributes(requestModel)
                .OfType<IHeadersProvider>()
                .ToArray();

            return GetHeaders(null, providers);
        }

        private static IDictionary<string, StringValues> GetHeaders(IDictionary<string, StringValues> headers, IReadOnlyList<IHeadersProvider> providers)
        {
            if (providers == null || !providers.Any())
                return null;

            if (headers == null)
                headers = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);

            foreach (var provider in providers)
            {
                var itemHeaders = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
                provider.Collect(itemHeaders);

                foreach (var itemHeader in itemHeaders)
                {
                    var values = headers.TryGetValue(itemHeader.Key, out var item) ? StringValues.Concat(item, itemHeader.Value) : itemHeader.Value;
                    headers[itemHeader.Key] = values;
                }
            }

            return headers;
        }

        private static IEnumerable<object> GetRequestAttributes(RequestModel requestModel)
        {
            return requestModel.ServiceModel.Attributes.Concat(requestModel.Attributes)
                .Concat(requestModel.Parameters.SelectMany(p => p.Attributes));
        }

        private static IDictionary<object, object> GetItems(RequestModel requestModel)
        {
            var providers = GetRequestAttributes(requestModel)
                .OfType<IItemsProvider>()
                .ToArray();
            return GetItems(null, providers);
        }

        private static IDictionary<object, object> GetItems(IDictionary<object, object> items, IReadOnlyList<IItemsProvider> providers)
        {
            if (providers == null || !providers.Any())
                return null;

            if (items == null)
                items = new Dictionary<object, object>();

            foreach (var provider in providers)
                provider.Collect(items);
            return items;
        }

        private RequestEntry GetRequestEntry(Type type, MethodInfo method)
        {
            return _requestEntries.GetOrAdd((type, method), key => CreateRequestEntry(type, method));
        }

        #endregion Private Method

        protected virtual RequestEntry CreateRequestEntry(Type type, MethodInfo method)
        {
            var serviceModel = _applicationModel.Services.SingleOrDefault(i => i.Type == type.GetTypeInfo());
            var requestModel = serviceModel.Requests.SingleOrDefault(r => r.MethodInfo == method);

            IEnumerable<IGoRequestOptionsProvider> GetRequestOptions()
            {
                yield return requestModel.Attributes.OfType<IGoRequestOptionsProvider>().FirstOrDefault();
                yield return requestModel.ServiceModel.Attributes.OfType<IGoRequestOptionsProvider>().FirstOrDefault();
                yield return DefaultRequestOptions.RequestOptions;
            }

            var entry = new RequestEntry
            {
                RequestModel = requestModel,
                Handler = null,
                Url = null,
                DefaultHeaders = GetHeaders(requestModel),
                DefaultItems = GetItems(requestModel),
                RequestOptions = GetRequestOptions().FirstOrDefault(o => o != null)
            };

            var url = entry.RequestModel.ServiceModel.Url.TrimEnd('/') + "/" + entry.RequestModel.Path.ToString().TrimStart('/');
            entry.Url = new Uri(url);

            entry.UrlVariableNames = TemplateEngine.GetVariables(url);
            entry.DefaultQuery = QueryHelpers.ParseNullableQuery(entry.Url.Query);
            var returnType = method.ReturnType;

            var isTask = typeof(Task).IsAssignableFrom(returnType);

            if (isTask)
            {
                var handler = Cache.GetHandler(this, entry.RequestModel.ResponseType);
                entry.Handler = handler;
            }
            else
                entry.Handler = Handle;

            return entry;
        }

        protected virtual async Task<object> SendAsync(GoRequestContext goRequestContext, RabbitRequestMessage requestMessage)
        {
            var requestEntry = goRequestContext.RequestEntry;
            try
            {
                var response = await _rabbitClient.SendAsync(requestMessage);
                return response.Body;
            }
            catch (RabbitClientException rce) when (rce.StatusCode == 404 && requestEntry.RequestOptions.NotFoundReturnNull)
            {
                return null;
            }
        }

        #region Help Type

        private static class Cache
        {
            #region Field

            private static readonly ConcurrentDictionary<Type, Func<GoRequestContext, Task>> Caches = new ConcurrentDictionary<Type, Func<GoRequestContext, Task>>();

            #endregion Field

            public static Func<GoRequestContext, Task> GetHandler(DefaultInterceptor interceptor, Type returnType)
            {
                var key = returnType;

                if (Caches.TryGetValue(key, out var handler))
                    return handler;

                var goRequestContextParameterExpression = Expression.Parameter(typeof(GoRequestContext), "goRequestContext");

                var method = typeof(DefaultInterceptor).GetMethod(nameof(HandleAsync), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(returnType);

                var callExpression = Expression.Call(Expression.Constant(interceptor), method, goRequestContextParameterExpression);

                handler = Expression.Lambda<Func<GoRequestContext, Task>>(callExpression, goRequestContextParameterExpression).Compile();

                Caches.TryAdd(key, handler);

                return handler;
            }
        }

        public struct RequestEntry
        {
            public IGoRequestOptionsProvider RequestOptions { get; set; }
            public RequestModel RequestModel { get; set; }
            public Uri Url { get; set; }
            public IReadOnlyList<string> UrlVariableNames { get; set; }
            public Func<GoRequestContext, object> Handler { get; set; }
            public IDictionary<object, object> DefaultItems { get; set; }

            public IDictionary<string, StringValues> DefaultHeaders { get; set; }
            public IDictionary<string, StringValues> DefaultQuery { get; set; }
        }

        public class GoRequestContext
        {
            public GoRequestContext(RequestEntry requestEntry, IInvocation invocation)
            {
                RequestEntry = requestEntry;
                RequestUrl = requestEntry.Url.ToString();

                if (requestEntry.DefaultHeaders != null)
                    Headers = new Dictionary<string, StringValues>(requestEntry.DefaultHeaders,
                        StringComparer.OrdinalIgnoreCase);

                if (requestEntry.DefaultItems != null)
                    Items = new Dictionary<object, object>(requestEntry.DefaultItems);

                if (requestEntry.DefaultQuery != null)
                    Query = new Dictionary<string, StringValues>(requestEntry.DefaultQuery,
                        StringComparer.OrdinalIgnoreCase);

                Arguments = AggregateArguments(invocation);
            }

            public IDictionary<string, object> Arguments { get; }
            public RequestEntry RequestEntry { get; }
            public object Body { get; set; }
            public string RequestUrl { get; set; }
            public IDictionary<string, StringValues> Query { get; set; }
            public IDictionary<string, StringValues> Headers { get; set; }
            public IDictionary<object, object> Items { get; set; }
            public IDictionary<string, object> PathVariables { get; set; }

            public GoRequestContext AppendQuery(IDictionary<string, string> values)
            {
                foreach (var item in values)
                {
                    AppendQuery(item.Key, item.Value);
                }

                return this;
            }

            public GoRequestContext AppendQuery(string key, StringValues value)
            {
                if (Query == null)
                    Query = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);

                value = Query.TryGetValue(key, out var temp) ? StringValues.Concat(temp, value) : value;
                Query[key] = value;

                return this;
            }

            public GoRequestContext AppendHeaders(IDictionary<string, string> values)
            {
                foreach (var item in values)
                {
                    AppendHeaders(item.Key, item.Value);
                }

                return this;
            }

            public GoRequestContext AppendHeaders(string key, StringValues value)
            {
                if (Headers == null)
                    Headers = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);

                value = Headers.TryGetValue(key, out var temp) ? StringValues.Concat(temp, value) : value;
                Headers[key] = value;

                return this;
            }

            public GoRequestContext AppendItems(object key, object value)
            {
                if (Items == null)
                    Items = new Dictionary<object, object>();

                Items[key] = value;

                return this;
            }

            public GoRequestContext AppendPathVariable(IDictionary<string, string> values)
            {
                foreach (var item in values)
                {
                    AppendPathVariable(item.Key, item.Value);
                }

                return this;
            }

            public GoRequestContext AppendPathVariable(string key, string value)
            {
                if (PathVariables == null)
                    PathVariables = new Dictionary<string, object>();
                PathVariables[key] = value;
                return this;
            }

            public GoRequestContext SetBody(object body)
            {
                Body = body;

                return this;
            }
        }

        private class DefaultRequestOptions : IGoRequestOptionsProvider
        {
            public static IGoRequestOptionsProvider RequestOptions { get; } = new DefaultRequestOptions();
            public bool NotFoundReturnNull { get; } = true;
        }

        #endregion Help Type
    }
}