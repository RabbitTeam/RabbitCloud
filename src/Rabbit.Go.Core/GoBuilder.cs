using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Rabbit.Go.Abstractions;
using Rabbit.Go.Abstractions.Codec;
using Rabbit.Go.Core;
using Rabbit.Go.Core.Codec;
using Rabbit.Go.Core.Internal;
using Rabbit.Go.Formatters;
using Rabbit.Go.Interceptors;
using Rabbit.Go.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Go
{
    public class GoBuilder
    {
        private HttpClient _client;
        private IKeyValueFormatterFactory _keyValueFormatterFactory;
        private IList<IInterceptorMetadata> _interceptors;
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
            if (_interceptors == null)
                _interceptors = new List<IInterceptorMetadata>();

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

        private static class Defaults
        {
            public static readonly HttpClient Client = new HttpClient();
            public static readonly ICodec Codec = JsonCodec.Instance;
            public static readonly IKeyValueFormatterFactory FormatterFactory = new KeyValueFormatterFactory();

            public static readonly IInterceptorMetadata[] Interceptors = Enumerable.Empty<IInterceptorMetadata>().ToArray();
        }

        public Go Build()
        {
            return new GoBuilderProx(_client ?? Defaults.Client, _codec ?? Defaults.Codec, _keyValueFormatterFactory ?? Defaults.FormatterFactory, _interceptors?.ToArray() ?? Defaults.Interceptors);
        }

        private class GoBuilderProx : GoBase
        {
            private readonly HttpClient _client;
            private readonly ICodec _codec;
            private readonly IKeyValueFormatterFactory _keyValueFormatterFactory;
            private readonly IReadOnlyList<IInterceptorMetadata> _interceptors;

            public GoBuilderProx(HttpClient client, ICodec codec, IKeyValueFormatterFactory keyValueFormatterFactory, IReadOnlyList<IInterceptorMetadata> interceptors)
            {
                _client = client;
                _codec = codec;
                _keyValueFormatterFactory = keyValueFormatterFactory;
                _interceptors = interceptors;
            }

            #region Overrides of GoBase

            private readonly IDictionary<Type, IServiceProvider> _services = new Dictionary<Type, IServiceProvider>();

            protected override IMethodInvoker CreateInvoker(Type type, MethodInfo methodInfo)
            {
                if (!_services.TryGetValue(type, out var services))
                {
                    services =
                        new ServiceCollection()
                            .AddOptions()
                            .AddGo(options =>
                            {
                                options.Types.Add(type);
                            })
                            .AddSingleton(_client)
                            .AddSingleton(_keyValueFormatterFactory)
                            .BuildServiceProvider();

                    var methodDescriptorCollectionProvider = services.GetRequiredService<IMethodDescriptorCollectionProvider>();

                    var interceptorDescriptors = _interceptors.Select(i => new InterceptorDescriptor(i)).ToArray();
                    foreach (var methodDescriptor in methodDescriptorCollectionProvider.Items)
                    {
                        methodDescriptor.Codec = _codec;
                        methodDescriptor.InterceptorDescriptors = interceptorDescriptors;
                    }

                    _services[type] = services;
                }

                var descriptors = services.GetRequiredService<IMethodDescriptorCollectionProvider>().Items;
                var descriptor = descriptors.SingleOrDefault(i =>
                    i.ClienType == type && i.MethodInfo == methodInfo);

                return services.GetRequiredService<IMethodInvokerFactory>().CreateInvoker(descriptor);
            }

            #endregion Overrides of GoBase
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
            return builder.Interceptor(context => context.GoContext.Request.Query(name, value));
        }
    }
}