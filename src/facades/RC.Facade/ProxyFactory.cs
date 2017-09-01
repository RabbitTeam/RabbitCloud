using Castle.DynamicProxy;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Rabbit.Cloud.Discovery.Abstractions;
using Rabbit.Cloud.Discovery.Client;
using Rabbit.Cloud.Facade.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade
{
    public class ProxyFactory
    {
        private readonly IDiscoveryClient _discoveryClient;

        public ProxyFactory(IDiscoveryClient discoveryClient)
        {
            _discoveryClient = discoveryClient;
        }

        public T GetProxy<T>()
        {
            var generator = new ProxyGenerator();

            var type = typeof(T);
            var handler = new DiscoveryHttpClientHandler(_discoveryClient, NullLogger<DiscoveryHttpClientHandler>.Instance);
            var httpClient = new HttpClient(handler);
            return (T)generator.CreateInterfaceProxyWithoutTarget(type, new[] { type }, new Interceptor(httpClient));
        }
    }

    internal class Interceptor : IInterceptor
    {
        private readonly HttpClient _httpClient;
        private static readonly MethodInfo HandleAsyncMethodInfo = typeof(Interceptor).GetMethod(nameof(HandleAsync), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);

        public Interceptor(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #region Implementation of IInterceptor

        public void Intercept(IInvocation invocation)
        {
            var returnType = invocation.Method.ReturnType;
            var isTask = typeof(Task).IsAssignableFrom(returnType);

            if (isTask)
            {
                returnType = returnType.GenericTypeArguments.FirstOrDefault() ?? typeof(object);
                invocation.ReturnValue = HandleAsyncMethodInfo.MakeGenericMethod(returnType).Invoke(this, new object[] { GetRequestMessage(invocation) });
            }
            else
            {
                invocation.ReturnValue = null;
            }
        }

        private async Task<T> HandleAsync<T>(HttpRequestMessage requestMessage)
        {
            var result = await _httpClient.SendAsync(requestMessage);
            return JsonConvert.DeserializeObject<T>(await result.Content.ReadAsStringAsync());
        }

        private object Handle(HttpRequestMessage requestMessage)
        {
            return null;
        }

        #endregion Implementation of IInterceptor

        #region Private Method

        private HttpRequestMessage GetRequestMessage(IInvocation invocation)
        {
            var mapping = invocation.Method.GetCustomAttribute<RequestMappingAttribute>();
            return new HttpRequestMessage(new HttpMethod(mapping.Method), GetUrl(invocation.Method, mapping));
        }

        private static string GetUrl(MemberInfo method, RequestMappingAttribute mapping)
        {
            var interfaceType = method.DeclaringType;

            var facadeClient = interfaceType.GetCustomAttribute<FacadeClientAttribute>();

            var url = facadeClient.Url ?? $"http://{facadeClient.Name}";

            return $"{url}{GetPath(method, mapping)}";
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