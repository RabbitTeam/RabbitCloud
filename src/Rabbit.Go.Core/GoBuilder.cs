using Microsoft.Extensions.Primitives;
using Rabbit.Go.Abstractions.Codec;
using Rabbit.Go.Formatters;
using Rabbit.Go.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
}