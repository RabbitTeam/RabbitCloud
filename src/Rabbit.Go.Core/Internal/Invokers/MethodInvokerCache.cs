using Rabbit.Go.Formatters;
using Rabbit.Go.Internal;
using System;
using System.Collections.Concurrent;
using System.Net.Http;

namespace Rabbit.Go.Core.Internal
{
    public class MethodInvokerCache
    {
        private readonly IGoClient _goClient;
        private readonly IKeyValueFormatterFactory _keyValueFormatterFactory;
        private readonly ITemplateParser _templateParser;
        private readonly IServiceProvider _services;

        private static readonly IGoClient DefaultGoClient = new HttpGoClient(new HttpClient());
        private static readonly IKeyValueFormatterFactory DefaultKeyValueFormatterFactory = new KeyValueFormatterFactory();
        private static readonly ITemplateParser DefaultTemplateParser = new TemplateParser();

        public MethodInvokerCache(
            IGoClient goClient = null,
            IKeyValueFormatterFactory keyValueFormatterFactory = null,
            ITemplateParser templateParser = null,
            IServiceProvider services = null)
        {
            _goClient = goClient ?? DefaultGoClient;
            _keyValueFormatterFactory = keyValueFormatterFactory ?? DefaultKeyValueFormatterFactory;
            _templateParser = templateParser ?? DefaultTemplateParser;
            _services = services;
        }

        private readonly ConcurrentDictionary<MethodDescriptor, MethodInvokerEntry> _caches = new ConcurrentDictionary<MethodDescriptor, MethodInvokerEntry>();

        public virtual MethodInvokerEntry Get(MethodDescriptor methodDescriptor)
        {
            var result = InterceptorFactory.GetAllInterceptors(methodDescriptor, _services);
            if (!_caches.TryGetValue(methodDescriptor, out var cache))
            {
                cache = new MethodInvokerEntry
                {
                    Client = _goClient,
                    Codec = methodDescriptor.Codec,
                    Interceptors = result.Interceptors,
                    KeyValueFormatterFactory = _keyValueFormatterFactory,
                    TemplateParser = _templateParser,
                    UrlTemplate = new UrlDescriptor(methodDescriptor.UrlTemplate.Template)
                };

                _caches[methodDescriptor] = cache;
            }
            cache.Interceptors = result.Interceptors;

            return cache;
        }
    }
}