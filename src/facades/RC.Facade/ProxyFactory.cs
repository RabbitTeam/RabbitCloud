using Castle.DynamicProxy;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Discovery.Client.Internal;
using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.Formatters;
using Rabbit.Cloud.Facade.Features;
using RC.Discovery.Client.Abstractions;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade
{
    public class ProxyFactory : IProxyFactory
    {
        private readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();
        private readonly Interceptor _interceptor;

        public ProxyFactory(RabbitRequestDelegate rabbitRequestDelegate, IOptions<FacadeOptions> facadeOptions)
        {
            _interceptor = new Interceptor(rabbitRequestDelegate, facadeOptions.Value);
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
        private readonly FacadeOptions _facadeOptions;
        private static readonly MethodInfo HandleAsyncMethodInfo = typeof(Interceptor).GetMethod(nameof(HandleAsync), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);

        public Interceptor(RabbitRequestDelegate rabbitRequestDelegate, FacadeOptions facadeOptions)
        {
            _rabbitRequestDelegate = rabbitRequestDelegate;
            _facadeOptions = facadeOptions;
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
                invocation.ReturnValue = Handle(invocation);
            }
        }

        private async Task<T> HandleAsync<T>(IInvocation invocation)
        {
            return (T)await InternalHandleAsync(invocation, typeof(T));
        }

        private object Handle(IInvocation invocation)
        {
            return InternalHandleAsync(invocation, invocation.Method.ReturnType).GetAwaiter().GetResult();
        }

        private async Task<object> InternalHandleAsync(IInvocation invocation, Type returnType)
        {
            var context = GetRabbitContext(invocation);

            await _rabbitRequestDelegate(context);

            return await Return(context, returnType);
        }

        private static RabbitContext GetRabbitContext(IInvocation invocation)
        {
            var context = new DefaultRabbitContext();
            context.Features.Set<IInvocationFeature>(new InvocationFeature(invocation));
            return context;
        }

        private async Task<object> Return(RabbitContext context, Type returnType)
        {
            using (var stream = await context.Response.Content.ReadAsStreamAsync())
            {
                var formatterContext = new OutputFormatterContext(context, returnType, stream);
                foreach (var formatter in _facadeOptions.OutputFormatters.Where(f => f.CanWriteResult(formatterContext)))
                {
                    var result = await formatter.WriteAsync(formatterContext);
                    if (result.IsModelSet)
                        return result.Model;
                }
            }
            return null;
        }

        #endregion Implementation of IInterceptor
    }
}