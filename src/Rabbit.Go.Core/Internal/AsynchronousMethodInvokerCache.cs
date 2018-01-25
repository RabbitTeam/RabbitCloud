using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Rabbit.Go.Codec;
using Rabbit.Go.Interceptors;
using Rabbit.Go.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Go.Internal
{
    public class AsynchronousMethodInvokerCache
    {
        private readonly ConcurrentDictionary<MethodDescriptor, RequestCacheEntry> _caches = new ConcurrentDictionary<MethodDescriptor, RequestCacheEntry>();

        public (RequestCacheEntry entry, IInterceptorMetadata[] interceptors) GetCachedResult(RequestContext requestContext)
        {
            var interceptors = InterceptorFactory.GetAllInterceptors(requestContext.MethodDescriptor, requestContext.RequestServices).Interceptors;
            if (_caches.TryGetValue(requestContext.MethodDescriptor, out var entry))
            {
                return (entry, interceptors);
            }

            var services = requestContext.RequestServices;
            var goOptions = services.GetService<IOptions<GoOptions>>().Value;
            var emptyRetryer = new EmptyRetryer();
            entry = new RequestCacheEntry(services.GetRequiredService<IGoClient>(), requestContext.MethodDescriptor, RequestOptions.Default, goOptions.ForamtterEncoder, goOptions.ForamtterDecoder, interceptors, () => emptyRetryer);

            var type = entry.MethodDescriptor.ClienType;
            var method = entry.MethodDescriptor.MethodInfo;

            entry.DefaultQuery
                .Merge(type.GetTypeAttributes<DefaultQueryAttribute>().ToDictionary(i => i.Name, i => i.Value))
                .Merge(method.GetTypeAttributes<DefaultQueryAttribute>().ToDictionary(i => i.Name, i => i.Value));

            entry.DefaultHeaders
                .Merge(type.GetTypeAttributes<DefaultHeaderAttribute>().ToDictionary(i => i.Name, i => i.Value))
                .Merge(method.GetTypeAttributes<DefaultHeaderAttribute>().ToDictionary(i => i.Name, i => i.Value));

            _caches[requestContext.MethodDescriptor] = entry;
            return (entry, interceptors);
        }
    }

    public class RequestCacheEntry
    {
        public RequestCacheEntry(IGoClient client, MethodDescriptor methodDescriptor, RequestOptions options, IEncoder encoder, IDecoder decoder, IEnumerable<IInterceptorMetadata> interceptors, Func<IRetryer> retryerFactory)
        {
            Client = client;
            MethodDescriptor = methodDescriptor;
            Options = options;
            Encoder = encoder;
            Decoder = decoder;
            Interceptors = interceptors;
            RetryerFactory = retryerFactory;
        }

        public IGoClient Client { get; }
        public MethodDescriptor MethodDescriptor { get; }
        public RequestOptions Options { get; }
        public IEncoder Encoder { get; }
        public IDecoder Decoder { get; }
        public IEnumerable<IInterceptorMetadata> Interceptors { get; }
        public Func<IRetryer> RetryerFactory { get; }
        public IDictionary<string, StringValues> DefaultQuery { get; } = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
        public IDictionary<string, StringValues> DefaultHeaders { get; } = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
    }
}