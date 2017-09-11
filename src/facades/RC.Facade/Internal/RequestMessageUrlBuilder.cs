using Rabbit.Cloud.Facade.Abstractions;
using System;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Rabbit.Cloud.Facade.Internal
{
    public class RequestMessageUrlBuilder : IRequestMessageBuilder
    {
        #region Implementation of IRequestMessageBuilder

        public void Build(RequestMessageBuilderContext context)
        {
            var method = context.Invocation.Method;
            var interfaceType = method.DeclaringType;
            var facadeClientAttribute = GetFacadeClientAttribute(interfaceType);

            if (facadeClientAttribute == null)
            {
                context.Canceled = true;
                return;
            }

            var baseUrl = GetBaseUrl(facadeClientAttribute);
            var requestMappingAttribute = GetRequestMappingAttribute(method);

            var pathAndQuery = GetPathAndQuery(requestMappingAttribute.Value, context.GetArgument);

            var uri = new Uri(new Uri(baseUrl), pathAndQuery);

            context.RequestMessage.RequestUri = uri;
            context.RequestMessage.Method = new HttpMethod(requestMappingAttribute.Method);
        }

        #endregion Implementation of IRequestMessageBuilder

        #region Private Method

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

        private static string GetPathAndQuery(string value, Func<string, object> getArgument)
        {
            return Regex.Replace(value, "{(\\w+)}", match =>
            {
                var key = match.Groups[1].Value;
                var argument = getArgument(key);

                return argument?.ToString();
            });
        }

        #endregion Private Method
    }
}