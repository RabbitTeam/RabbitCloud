using Castle.DynamicProxy;
using Microsoft.Extensions.Options;
using Rabbit.Go.Abstractions.Codec;
using Rabbit.Go.Core;
using Rabbit.Go.Core.Internal;
using Rabbit.Go.Formatters;
using Rabbit.Go.Interceptors;
using Rabbit.Go.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Rabbit.Go
{
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
                new DefaultMethodDescriptorProvider(new[] { type }, new[] { goModelProvider }, options);

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
}