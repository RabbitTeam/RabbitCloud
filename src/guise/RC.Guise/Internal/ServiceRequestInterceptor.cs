using Castle.DynamicProxy;
using Newtonsoft.Json;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Http;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Guise.Internal
{
    internal class ServiceRequestInterceptor : IInterceptor
    {
        #region Field

        private readonly RabbitRequestDelegate _rabbitRequestDelegate;
        private readonly IServiceProvider _services;
        private static readonly MethodInfo HandleAsyncMethodInfo = typeof(ServiceRequestInterceptor).GetMethod(nameof(HandleAsync), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);

        #endregion Field

        #region Constructor

        public ServiceRequestInterceptor(RabbitRequestDelegate rabbitRequestDelegate, IServiceProvider services)
        {
            _rabbitRequestDelegate = rabbitRequestDelegate;
            _services = services;
        }

        #endregion Constructor

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

        #endregion Implementation of IInterceptor

        #region Private Method

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
            // build RabbitContext
            var context = (HttpRabbitContext)GetRabbitContext(invocation);

            var method = invocation.Method;

            // send service request
            await RequestAsync(context);

            // read result from resposne
            return await ReturnAsync(context, method.ReturnType);
        }

        private IRabbitContext GetRabbitContext(IInvocation invocation)
        {
            var context = new HttpRabbitContext();
            /*context.Features.Set<IInvocationFeature>(new InvocationFeature(invocation));
            var serviceDescriptor = _services.GetRequiredService<IServiceDescriptorCollectionProvider>().ServiceDescriptors.GetServiceDescriptor(invocation.Method.GetHashCode());
            context.Features.Set<IServiceDescriptorFeature>(new ServiceDescriptorFeature(serviceDescriptor));*/
            return context;
        }

        private async Task RequestAsync(HttpRabbitContext rabbitContext)
        {
            await _rabbitRequestDelegate(rabbitContext);
        }

        private Task<object> ReturnAsync(HttpRabbitContext rabbitContext, Type returnType)
        {
            var response = rabbitContext.Response;
            using (var stream = response.Body)
            {
                using (var reader = new StreamReader(stream))
                {
                    return Task.FromResult(JsonConvert.DeserializeObject(reader.ReadToEnd(), returnType));
                }
                /*                        var formatterContext = new OutputFormatterContext(rabbitContext, returnType, stream);

                                        var formatters = _facadeOptions.OutputFormatters.Where(f => f.CanWriteResult(formatterContext)).ToArray();

                                        if (!formatters.Any())
                                            throw new NotSupportedException("not find formatter.");

                                        foreach (var formatter in formatters)
                                        {
                                            var result = await formatter.WriteAsync(formatterContext);
                                            if (!result.IsModelSet)
                                                continue;
                                            resultExecutedContext.Result = result.Model;
                                            break;
                                        }*/
            }
        }

        #endregion Private Method
    }
}