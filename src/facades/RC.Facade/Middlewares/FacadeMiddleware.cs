using Castle.DynamicProxy;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Features;
using RC.Discovery.Client.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade.Middlewares
{
    public class FacadeMiddleware
    {
        private readonly RabbitRequestDelegate _next;

        public FacadeMiddleware(RabbitRequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(RabbitContext context)
        {
            var invocationFeature = context.Features.Get<IInvocationFeature>();
            var invocation = invocationFeature.Invocation;

            SetRequest(context, invocation);

            await _next(context);
        }

        #region Private Method

        private class ServiceUrlBuilder
        {
            public ServiceUrlBuilder(IInvocation invocation)
            {
                Headers = new Dictionary<string, object>();
                Query = new QueryBuilder();

                var method = invocation.Method;
                var items = method.GetParameters().ToDictionary(i => i, i => i.GetCustomAttributes().OfType<IBindingSourceMetadata>().ToArray());

                var index = -1;
                foreach (var item in items)
                {
                    index = index + 1;
                    var key = item.Key.Name.ToLower();
                    var value = invocation.Arguments[index];

                    var bindings = item.Value;

                    if (bindings == null || !bindings.Any())
                    {
                        Query.Add(key, value.ToString());
                    }
                    else
                    {
                        foreach (var binding in bindings)
                        {
                            if (binding is FromQueryAttribute)
                            {
                                Query.Add(key, value.ToString());
                            }
                            else if (binding is FromHeaderAttribute)
                            {
                                Headers[key] = value;
                            }
                            else if (binding is FromBodyAttribute)
                            {
                                Body = new KeyValuePair<string, object>(key, value);
                            }
                        }
                    }
                }
            }

            public IDictionary<string, object> Headers { get; }
            public QueryBuilder Query { get; }
            public KeyValuePair<string, object> Body { get; }
        }

        private static void SetRequest(RabbitContext context, IInvocation invocation)
        {
            var mapping = invocation.Method.GetCustomAttribute<RequestMappingAttribute>();
            if (mapping == null)
                return;

            var builder = new ServiceUrlBuilder(invocation);

            var method = invocation.Method;
            var interfaceType = method.DeclaringType;

            var facadeClient = interfaceType.GetCustomAttribute<FacadeClientAttribute>();

            var url = facadeClient.Url ?? $"http://{facadeClient.Name}";

            url = $"{url}{GetPath(method, mapping)}";

            url = url + builder.Query;

            var request = context.Request;
            request.Method = new HttpMethod(mapping.Method);
            request.RequestUri = new Uri(url);

            foreach (var header in builder.Headers)
                request.Headers.Add(header.Key, header.Value.ToString());

            if (builder.Body.Value != null)
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(builder.Body.Value));
            }
        }

        private static string GetPath(MemberInfo method, RequestMappingAttribute mapping)
        {
            if (!string.IsNullOrEmpty(mapping.Value))
                return mapping.Value;

            var interfaceType = method.DeclaringType;

            var typeNmae = interfaceType.Name;

            typeNmae = typeNmae.TrimStart('I');
            if (typeNmae.EndsWith("Service"))
                typeNmae = typeNmae.Substring(0, typeNmae.Length - 7);

            var methodName = method.Name;

            if (methodName.EndsWith("Async"))
                methodName = methodName.Substring(0, methodName.Length - 5);

            return $"/{typeNmae}/{methodName}";
        }

        #endregion Private Method
    }
}