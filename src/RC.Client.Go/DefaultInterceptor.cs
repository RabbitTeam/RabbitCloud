using Castle.DynamicProxy;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Client.Go.Abstractions;
using Rabbit.Cloud.Client.Go.ApplicationModels;
using System;
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

            public GoRequestContext AppendQuery(string key, StringValues value)
            {
                if (Query == null)
                    Query = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);

                value = Query.TryGetValue(key, out var temp) ? StringValues.Concat(temp, value) : value;
                Query[key] = value;

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
        }

        private async Task<object> InternalHandleAsync(GoRequestContext goRequestContext)
        {
            var requestEntry = goRequestContext.RequestEntry;
            var requestModel = requestEntry.RequestModel;

            var requestType = requestModel.RequesType;
            var responseType = requestModel.ResponseType;

            foreach (var parameterModel in requestModel.Parameters)
            {
                var name = parameterModel.ParameterName;
                var value = goRequestContext.Arguments[parameterModel.ParameterInfo.Name];
                switch (parameterModel.Target)
                {
                    case ParameterTarget.Query:
                        goRequestContext.AppendQuery(name, value?.ToString());
                        break;

                    case ParameterTarget.Body:
                        goRequestContext.Body = value;
                        break;

                    case ParameterTarget.Header:
                        goRequestContext.AppendHeaders(name, value?.ToString());
                        break;

                    case ParameterTarget.Items:
                        goRequestContext.AppendItems(name, value);
                        break;

                    case ParameterTarget.Path:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var templateVariables = requestModel.Parameters.Where(i => i.Target == ParameterTarget.Path).ToDictionary(i => i.ParameterInfo.Name,
                i => goRequestContext.Arguments[i.ParameterInfo.Name]);
            var renderResult = _templateEngine.Render(goRequestContext.RequestUrl, templateVariables);
            var requestUrl = QueryHelpers.AddQueryString(renderResult?.Result ?? goRequestContext.RequestUrl,
                goRequestContext.Query.ToDictionary(i => i.Key, i => i.Value.ToString()));

            var requestMessage = new RabbitRequestMessage(requestType, responseType, new Uri(requestUrl), goRequestContext.Body, goRequestContext.Headers, goRequestContext.Items);
            var response = await _rabbitClient.SendAsync(requestMessage);
            return response.Body;

            /*            var request = model.RequestParameterIndex == -1 ? null : invocation.Arguments[model.RequestParameterIndex];
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
                        }*/
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
            var providers = requestModel.ServiceModel.Attributes.Concat(requestModel.Attributes)
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

        private static IDictionary<object, object> GetItems(RequestModel requestModel)
        {
            var providers = requestModel.ServiceModel.Attributes.Concat(requestModel.Attributes)
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

        /*        private string FindBodyParameterName(RequestModel requestModel)
                {
                    foreach (var parameterModel in requestModel.Parameters)
                    {
                        if (parameterModel.Attributes.OfType<GoBodyAttribute>() == null)
                            continue;
                        return parameterModel.ParameterInfo.Name;
                    }

                    return requestModel.Parameters.LastOrDefault()?.ParameterInfo.Name;
                }*/

        private RequestEntry GetRequestEntry(Type type, MethodInfo method)
        {
            return _requestEntries.GetOrAdd((type, method), key =>
            {
                var serviceModel = _applicationModel.Services.SingleOrDefault(i => i.Type == type.GetTypeInfo());
                var requestModel = serviceModel.Requests.SingleOrDefault(r => r.MethodInfo == method);
                var entry = new RequestEntry
                {
                    RequestModel = requestModel,
                    Handler = null,
                    Url = null,
                    DefaultHeaders = GetHeaders(requestModel),
                    DefaultItems = GetItems(requestModel)
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
            });
        }

        #endregion Private Method

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
            public RequestModel RequestModel { get; set; }
            public Uri Url { get; set; }
            public IReadOnlyList<string> UrlVariableNames { get; set; }
            public Func<GoRequestContext, object> Handler { get; set; }
            public IDictionary<object, object> DefaultItems { get; set; }

            public IDictionary<string, StringValues> DefaultHeaders { get; set; }
            public IDictionary<string, StringValues> DefaultQuery { get; set; }
        }

        #endregion Help Type
    }
}