using Microsoft.AspNetCore.WebUtilities;
using Rabbit.Cloud.Facade.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Rabbit.Cloud.Facade.Internal
{
    public class RequestMessageUrlBuilder : RequestMessageBuilder
    {
        #region Overrides of RequestMessageBuilder

        public override void Build(RequestMessageBuilderContext context)
        {
            var method = context.Method;
            var interfaceType = method.DeclaringType;
            var facadeClientAttribute = GetFacadeClientAttribute(interfaceType);

            if (facadeClientAttribute == null)
            {
                context.Canceled = true;
                return;
            }

            var baseUrl = GetBaseUrl(facadeClientAttribute);
            var requestMappingAttribute = GetRequestMappingAttribute(method);
            var request = context.RequestMessage;
            request.Method = new HttpMethod(requestMappingAttribute.Method);

            string GetQuery(string key)
            {
                return context.GetArgument(key)?.ToString();
            }

            // interface , method ToQuery
            var querys = GetQuerys(interfaceType).Concat(GetQuerys(method)).Select(i => new KeyValuePair<string, string>(i.Name, i.Value)).ToArray();

            // arguments ToQuery
            var parameterQuerys = GetQuerysByParameter(request.Method, method.GetParameters(), GetQuery).Select(i => new KeyValuePair<string, string>(i.Name, i.Value)).ToArray();

            // placeholder keys
            var parameterKeys = GetPlaceholders(requestMappingAttribute.Value).ToArray();

            // resolve placeholder
            var pathAndQuery = BuildPathAndQuery(requestMappingAttribute.Value,
                parameterKeys.ToDictionary(i => i, GetQuery));

            // remove placeholder querys
            var result = querys.Concat(parameterQuerys.Where(i => !parameterKeys.Contains(i.Key)))
                .ToDictionary(i => i.Key, i => i.Value);

            request.RequestUri = new Uri(new Uri(baseUrl), QueryHelpers.AddQueryString(pathAndQuery, result));
        }

        #endregion Overrides of RequestMessageBuilder

        #region Private Method

        private static IEnumerable<ToQueryAttribute> GetQuerys(MemberInfo methodInfo)
        {
            return methodInfo.GetCustomAttributes<ToQueryAttribute>();
        }

        private static IEnumerable<ToQueryAttribute> GetQuerysByParameter(HttpMethod httpMethod, IEnumerable<ParameterInfo> parameterInfos, Func<string, string> getQuery)
        {
            foreach (var parameterInfo in parameterInfos)
            {
                var metadatas = parameterInfo.GetCustomAttributes(false).OfType<IBuilderMetadata>().ToArray();
                var toQueryAttribute = metadatas.OfType<ToQueryAttribute>().LastOrDefault();

                if (toQueryAttribute != null)
                    yield return toQueryAttribute;

                if (metadatas.Any())
                    continue;

                if (httpMethod != HttpMethod.Get && httpMethod != HttpMethod.Head)
                    continue;

                if (toQueryAttribute == null)
                    toQueryAttribute = new ToQueryAttribute(parameterInfo.Name);
                toQueryAttribute.Value = getQuery(parameterInfo.Name);

                yield return toQueryAttribute;
            }
        }

        private static string BuildPathAndQuery(string pathAndQuery, IDictionary<string, string> values)
        {
            if (values == null || !values.Any())
                return pathAndQuery;

            var builder = new StringBuilder(pathAndQuery);
            foreach (var value in values)
            {
                builder.Replace($"{{{value.Key}}}", value.Value);
            }

            return builder.ToString();
        }

        private static IEnumerable<string> GetPlaceholders(string value)
        {
            foreach (Match match in Regex.Matches(value, "{(\\w+)}"))
            {
                var key = match.Groups[1].Value;
                yield return key;
            }
        }

        private static FacadeClientAttribute GetFacadeClientAttribute(Type interfaceType)
        {
            if (!interfaceType.IsInterface)
                return null;

            var facadeClientAttribute = interfaceType.GetCustomAttribute<FacadeClientAttribute>();

            if (facadeClientAttribute == null)
                return null;

            if (string.IsNullOrEmpty(facadeClientAttribute.Name) && string.IsNullOrEmpty(facadeClientAttribute.Url))
            {
                var typeName = interfaceType.Name;
                if (typeName.StartsWith("I"))
                    typeName = typeName.Remove(0, 1);
                facadeClientAttribute.Name = typeName;
            }

            return facadeClientAttribute;
        }

        private static RequestMappingAttribute GetRequestMappingAttribute(MemberInfo method)
        {
            var requestMappingAttribute = method.GetCustomAttribute<RequestMappingAttribute>() ?? new RequestMappingAttribute();

            if (string.IsNullOrEmpty(requestMappingAttribute.Method))
                requestMappingAttribute.Method = HttpMethod.Get.Method;

            if (string.IsNullOrEmpty(requestMappingAttribute.Value))
            {
                var methodName = method.Name;
                if (methodName.EndsWith("Async", StringComparison.OrdinalIgnoreCase))
                {
                    methodName = methodName.Substring(0, methodName.Length - 5);
                }
                requestMappingAttribute.Value = methodName;
            }

            return requestMappingAttribute;
        }

        private static string GetBaseUrl(FacadeClientAttribute facadeClientAttribute)
        {
            var url = facadeClientAttribute.Url ?? $"http://{facadeClientAttribute.Name}";
            return url;
        }

        #endregion Private Method
    }
}