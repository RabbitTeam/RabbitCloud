using Castle.DynamicProxy;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Rabbit.Go.Abstractions.Codec;
using Rabbit.Go.Core;
using Rabbit.Go.Core.Internal;
using Rabbit.Go.Formatters;
using Rabbit.Go.Interceptors;
using Rabbit.Go.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Go
{
    public class GoBuilder
    {
        public GoBuilder()
        {
            _interceptors = new List<IInterceptorMetadata>();
        }

        private HttpClient _client;
        private IKeyValueFormatterFactory _keyValueFormatterFactory;
        private readonly IList<IInterceptorMetadata> _interceptors;
        private ICodec _codec;

        public GoBuilder Codec(ICodec codec)
        {
            _codec = codec;

            return this;
        }

        public GoBuilder Client(HttpClient client)
        {
            _client = client;
            return this;
        }

        public GoBuilder Client(HttpMessageHandler messageHandler)
        {
            _client = new HttpClient(messageHandler);
            return this;
        }

        public GoBuilder KeyValueFormatterFactory(IKeyValueFormatterFactory keyValueFormatterFactory)
        {
            _keyValueFormatterFactory = keyValueFormatterFactory;
            return this;
        }

        public GoBuilder Interceptors(params IInterceptorMetadata[] interceptors)
        {
            foreach (var interceptor in interceptors)
                _interceptors.Add(interceptor);

            return this;
        }

        public GoBuilder Interceptor(Action<RequestExecutingContext> interceptorDelegate)
        {
            return Interceptors(new DelegateRequestInterceptor(interceptorDelegate));
        }

        public object Target(Type type)
        {
            return Build().CreateInstance(type);
        }

        public Go Build()
        {
            return new Go(_keyValueFormatterFactory, _client, _codec, _interceptors.Distinct().ToArray());
        }

        #region Help Type

        private class DelegateRequestInterceptor : IAsyncRequestInterceptor
        {
            private readonly Action<RequestExecutingContext> _interceptor;

            public DelegateRequestInterceptor(Action<RequestExecutingContext> interceptor)
            {
                _interceptor = interceptor;
            }

            #region Implementation of IAsyncRequestInterceptor

            public async Task OnActionExecutionAsync(RequestExecutingContext context, RequestExecutionDelegate next)
            {
                _interceptor(context);

                await next();
            }

            #endregion Implementation of IAsyncRequestInterceptor
        }

        #endregion Help Type
    }

    public static class GoBuilderExtensions
    {
        public static T Target<T>(this GoBuilder builder)
        {
            return (T)builder.Target(typeof(T));
        }

        public static GoBuilder Query(this GoBuilder builder, string name, StringValues value)
        {
            return builder.Interceptor(context => context.RequestBuilder.Query(name, value));
        }
    }

    public static class GoExtensions
    {
        public static T CreateInstance<T>(this Go go)
        {
            return (T)go.CreateInstance(typeof(T));
        }
    }

    public class Go
    {
        private readonly IKeyValueFormatterFactory _keyValueFormatterFactory;
        private readonly HttpClient _client;
        private readonly ICodec _codec;
        private readonly IReadOnlyList<IInterceptorMetadata> _interceptors;

        public Go(IKeyValueFormatterFactory keyValueFormatterFactory, HttpClient client, ICodec codec, IReadOnlyList<IInterceptorMetadata> interceptors)
        {
            _keyValueFormatterFactory = keyValueFormatterFactory;
            _client = client;
            _codec = codec;
            _interceptors = interceptors;
        }

        private IDictionary<MethodInfo, Func<IMethodInvoker>> CreateMethodInvokerFactory(Type type)
        {
            IMethodInvokerFactory methodInvokerFactory = new DefaultMethodInvokerFactory(new MethodInvokerCache(_client, _keyValueFormatterFactory, null, null));

            var options = Options.Create(new GoOptions());

            var goModelProvider = new DefaultGoModelProvider(options);

            var descriptorProviderContext = new MethodDescriptorProviderContext();

            var defaultMethodDescriptorProvider =
                new DefaultMethodDescriptorProvider(new[] { type }, new[] { goModelProvider });

            defaultMethodDescriptorProvider.OnProvidersExecuting(descriptorProviderContext);

            var descriptors = descriptorProviderContext.Results;

            var interceptorDescriptors = _interceptors.Select(i => new InterceptorDescriptor(i)).ToArray();
            foreach (var descriptor in descriptors)
            {
                if (_codec != null)
                    descriptor.Codec = _codec;
                descriptor.InterceptorDescriptors = descriptor.InterceptorDescriptors.Concat(interceptorDescriptors).ToArray();
            }

            return descriptors.ToDictionary(i => i.MethodInfo,
                i => new Func<IMethodInvoker>(() => methodInvokerFactory.CreateInvoker(i)));
        }

        private readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();

        public object CreateInstance(Type type)
        {
            var methodInvokerFactoryTable = CreateMethodInvokerFactory(type);

            return _proxyGenerator.CreateInterfaceProxyWithoutTarget(type, Enumerable.Empty<Type>().ToArray(), new InterceptorAsync(async invocation =>
            {
                var invokerFactory = methodInvokerFactoryTable[invocation.Method];
                var invoker = invokerFactory();

                var result = await invoker.InvokeAsync(invocation.Arguments);
                return result;
            }));
        }
    }

    public class InterceptorAsync : IInterceptor
    {
        private readonly Func<IInvocation, Task<object>> _invoker;

        public InterceptorAsync(Func<IInvocation, Task<object>> invoker)
        {
            _invoker = invoker;
        }

        #region Implementation of IInterceptor

        public virtual void Intercept(IInvocation invocation)
        {
            var returnType = invocation.Method.ReturnType;

            var isTask = typeof(Task).IsAssignableFrom(returnType);

            if (returnType == typeof(void))
                returnType = null;
            else if (isTask)
                returnType = returnType.IsGenericType ? returnType.GenericTypeArguments[0] : null;

            object result;
            if (isTask)
            {
                result = returnType == null ? HandleTaskAsync(invocation) : Cache.GetHandler(returnType)(this, invocation);
            }
            else
            {
                result = Handle(invocation);
            }

            if (result != null)
                invocation.ReturnValue = result;
        }

        #endregion Implementation of IInterceptor

        private async Task HandleTaskAsync(IInvocation invocation)
        {
            await DoHandleAsync(invocation);
        }

        private async Task<T> HandleAsync<T>(IInvocation invocation)
        {
            var value = await DoHandleAsync(invocation);
            if (value is Task<T> task)
                return await task;
            return (T)value;
        }

        private object Handle(IInvocation invocation)
        {
            return DoHandleAsync(invocation).GetAwaiter().GetResult();
        }

        private async Task<object> DoHandleAsync(IInvocation invocation)
        {
            return await _invoker(invocation);
        }

        #region Help Type

        private static class Cache
        {
            #region Field

            private static readonly ConcurrentDictionary<Type, Func<InterceptorAsync, IInvocation, Task>> Caches = new ConcurrentDictionary<Type, Func<InterceptorAsync, IInvocation, Task>>();

            #endregion Field

            public static Func<InterceptorAsync, IInvocation, Task> GetHandler(Type returnType)
            {
                var key = returnType;

                if (Caches.TryGetValue(key, out var handler))
                    return handler;

                var interceptorParameterExpression = Expression.Parameter(typeof(InterceptorAsync), "interceptor");
                var parameterExpression = Expression.Parameter(typeof(IInvocation), "invocation");

                var method = typeof(InterceptorAsync).GetMethod(nameof(HandleAsync), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(returnType);

                var callExpression = Expression.Call(interceptorParameterExpression, method, parameterExpression);

                handler = Expression.Lambda<Func<InterceptorAsync, IInvocation, Task>>(callExpression, interceptorParameterExpression, parameterExpression).Compile();

                Caches.TryAdd(key, handler);

                return handler;
            }
        }

        #endregion Help Type
    }
}