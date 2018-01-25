using Castle.DynamicProxy;
using Microsoft.Extensions.Primitives;
using Rabbit.Go.Codec;
using Rabbit.Go.Formatters;
using Rabbit.Go.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Go
{
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

    public class MethodInvokerProvider
    {
        private readonly AsynchronousMethodInvokerCache _invokerCache;
        private readonly IServiceProvider _services;
        private readonly ITemplateParser _templateParser = new TemplateParser();

        public MethodInvokerProvider(AsynchronousMethodInvokerCache invokerCache, IServiceProvider services)
        {
            _invokerCache = invokerCache;
            _services = services;
        }

        private RequestContext CreateRequestContext(IDictionary<ParameterTarget, IDictionary<string, StringValues>> formatResult, MethodDescriptor descriptor, IReadOnlyList<object> arguments)
        {
            var pathArguments = formatResult[ParameterTarget.Path].ToDictionary(i => i.Key, i => i.Value.ToString());
            var requestUri = _templateParser.Parse(descriptor.Uri, pathArguments);

            var requestContext = new RequestContext(requestUri)
            {
                Method = descriptor.Method,
                MethodDescriptor = descriptor,
                RequestServices = _services
            };

            return requestContext;
        }

        public async Task<AsynchronousMethodInvoker> CreateMethodHandlerAsync(MethodDescriptor descriptor, object[] arguments)
        {
            var formatResult = await FormatAsync(descriptor, new KeyValueFormatterFactory(), arguments);
            var requestContext = CreateRequestContext(formatResult, descriptor, arguments);

            var result = _invokerCache.GetCachedResult(requestContext);
            var entry = result.entry;

            // set default headers and query
            foreach (var item in entry.DefaultHeaders)
                requestContext.AppendHeader(item.Key, item.Value);
            foreach (var item in entry.DefaultQuery)
                requestContext.AppendQuery(item.Key, item.Value);

            BuildQueryAndHeaders(requestContext, formatResult);

            await BuildBodyAsync(requestContext, entry.Encoder, descriptor.Parameters, arguments);

            var methodHandler = new AsynchronousMethodInvoker(requestContext, entry.RetryerFactory(), entry);

            return methodHandler;
        }

        private static async Task<IDictionary<ParameterTarget, IDictionary<string, StringValues>>> FormatAsync(MethodDescriptor methodDescriptor, IKeyValueFormatterFactory keyValueFormatterFactory, IReadOnlyList<object> arguments)
        {
            IDictionary<ParameterTarget, IDictionary<string, StringValues>> formatResult =
                new Dictionary<ParameterTarget, IDictionary<string, StringValues>>
                {
                    {
                        ParameterTarget.Query,
                        new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase)
                    },
                    {ParameterTarget.Path, new Dictionary<string, StringValues>()},
                    {
                        ParameterTarget.Header,
                        new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase)
                    }
                };

            var parameterDescriptors = methodDescriptor.Parameters;

            for (var i = 0; i < parameterDescriptors.Count; i++)
            {
                var parameterDescriptor = parameterDescriptors[i];

                if (!formatResult.TryGetValue(parameterDescriptor.Target, out var itemResult))
                    continue;

                var parameter = parameterDescriptors[i];
                var value = arguments[i];
                var item = await keyValueFormatterFactory.FormatAsync(value, parameter.ParameterType, parameterDescriptor.Name);

                foreach (var t in item)
                    itemResult[t.Key] = t.Value;
            }

            return formatResult;
        }

        private static void BuildQueryAndHeaders(RequestContext context, IDictionary<ParameterTarget, IDictionary<string, StringValues>> parameters)
        {
            foreach (var item in parameters)
            {
                var target = item.Value;
                IDictionary<string, StringValues> source;
                switch (item.Key)
                {
                    case ParameterTarget.Query:
                        source = context.Query;
                        break;

                    case ParameterTarget.Header:
                        source = context.Headers;
                        break;

                    default:
                        continue;
                }

                foreach (var t in target)
                {
                    source[t.Key] = t.Value;
                }
            }
        }

        private static async Task BuildBodyAsync(RequestContext requestContext, IEncoder encoder, IReadOnlyList<ParameterDescriptor> parameterDescriptors, object[] arguments)
        {
            if (encoder == null)
                return;

            object bodyArgument = null;
            Type bodyType = null;
            for (var i = 0; i < parameterDescriptors.Count; i++)
            {
                var parameterDescriptor = parameterDescriptors[i];
                if (parameterDescriptor.Target != ParameterTarget.Body)
                    continue;

                bodyArgument = arguments[i];
                bodyType = parameterDescriptor.ParameterType;
                break;
            }

            if (bodyArgument == null || bodyType == null)
                return;

            try
            {
                await encoder.EncodeAsync(bodyArgument, bodyType, requestContext);
            }
            catch (EncodeException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new EncodeException(e.Message, e);
            }
        }
    }

    public class DefaultGoFactory : IGoFactory
    {
        private readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();
        private readonly MethodInvokerProvider _invokerProvider;
        private readonly IReadOnlyList<MethodDescriptor> _methodDescriptors;

        public DefaultGoFactory(MethodInvokerProvider invokerProvider, IMethodDescriptorCollectionProvider methodDescriptorCollectionProvider)
        {
            _invokerProvider = invokerProvider;
            _methodDescriptors = methodDescriptorCollectionProvider.Items;
        }

        #region Overrides of IGoFactory

        public object CreateInstance(Type type)
        {
            return _proxyGenerator.CreateInterfaceProxyWithoutTarget(type, new Type[0], new InterceptorAsync(async invocation =>
            {
                var invoker = await _invokerProvider.CreateMethodHandlerAsync(GetMethodDescriptor(invocation), invocation.Arguments);
                var result = await invoker.InvokeAsync();

                return result;
            }));
        }

        #endregion Overrides of IGoFactory

        #region Private Method

        private static Type GetProxyType(IInvocation context)
        {
            //todo: think of a more reliable way
            var proxyType = context.Proxy.GetType();
            var name = proxyType.Name.Substring(0, proxyType.Name.Length - 5);
            return proxyType.GetInterface(name);
        }

        private MethodDescriptor GetMethodDescriptor(IInvocation invocation)
        {
            var type = GetProxyType(invocation);
            var method = invocation.Method;

            return _methodDescriptors.SingleOrDefault(i => i.ClienType == type && i.MethodInfo == method);
        }

        #endregion Private Method
    }
}