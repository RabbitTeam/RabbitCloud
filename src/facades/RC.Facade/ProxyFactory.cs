using Castle.DynamicProxy;
using Newtonsoft.Json;
using Rabbit.Cloud.Discovery.Client.Internal;
using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Features;
using RC.Discovery.Client.Abstractions;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade
{
    public class ProxyFactory : IProxyFactory
    {
        private readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();
        private readonly Interceptor _interceptor;

        public ProxyFactory(RabbitRequestDelegate rabbitRequestDelegate)
        {
            _interceptor = new Interceptor(rabbitRequestDelegate);
        }

        #region Implementation of IProxyFactory

        public T GetProxy<T>()
        {
            var type = typeof(T);
            return (T)_proxyGenerator.CreateInterfaceProxyWithoutTarget(type, new[] { type }, _interceptor);
        }

        #endregion Implementation of IProxyFactory
    }

    internal class Interceptor : IInterceptor
    {
        private readonly RabbitRequestDelegate _rabbitRequestDelegate;
        private static readonly MethodInfo HandleAsyncMethodInfo = typeof(Interceptor).GetMethod(nameof(HandleAsync), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);

        public Interceptor(RabbitRequestDelegate rabbitRequestDelegate)
        {
            _rabbitRequestDelegate = rabbitRequestDelegate;
        }

        #region Implementation of IInterceptor

        public void Intercept(IInvocation invocation)
        {
            var returnType = invocation.Method.ReturnType;
            var isTask = typeof(Task).IsAssignableFrom(returnType);

            if (isTask)
            {
                returnType = returnType.GenericTypeArguments.FirstOrDefault() ?? typeof(object);
                invocation.ReturnValue = HandleAsyncMethodInfo.MakeGenericMethod(returnType).Invoke(this, new object[] { invocation });
            }
            else
            {
                invocation.ReturnValue = null;
            }
        }

        private async Task<T> HandleAsync<T>(IInvocation invocation)
        {
            var context = new DefaultRabbitContext();

            context.Features.Set<IInvocationFeature>(new InvocationFeature(invocation));

            await _rabbitRequestDelegate(context);

            return JsonConvert.DeserializeObject<T>(await context.Response.Content.ReadAsStringAsync());
        }

        private object Handle(HttpRequestMessage requestMessage)
        {
            throw new NotSupportedException();
        }

        #endregion Implementation of IInterceptor

        /*        #region Private Method

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

                #endregion Private Method*/
    }
}