using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Features;
using RC.Discovery.Client.Abstractions;
using System;
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

            var requestMapping = invocation.Method.GetCustomAttribute<RequestMappingAttribute>();

            var request = context.Request;

            request.RequestUri = new Uri(GetUrl(invocation.Method, requestMapping));
            request.Method = new HttpMethod(requestMapping.Method);

            await _next(context);
        }

        #region Private Method

        private string GetUrl(MemberInfo method, RequestMappingAttribute mapping)
        {
            var interfaceType = method.DeclaringType;

            var facadeClient = interfaceType.GetCustomAttribute<FacadeClientAttribute>();

            var url = facadeClient.Url ?? $"http://{facadeClient.Name}";

            return $"{url}{GetPath(method, mapping)}";
        }

        private string GetPath(MemberInfo method, RequestMappingAttribute mapping)
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