using Microsoft.Extensions.Primitives;
using Rabbit.Go.Codec;
using Rabbit.Go.Formatters;
using Rabbit.Go.Interceptors;
using System;
using System.Collections.Generic;

namespace Rabbit.Go
{
    public class RequestCache
    {
        public IReadOnlyList<IInterceptorMetadata> Interceptors { get; set; }
        public IEncoder Encoder { get; set; }
        public IDecoder Decoder { get; set; }
        public RequestOptions RequestOptions { get; set; }
        public IKeyValueFormatterFactory KeyValueFormatterFactory { get; set; }
        public IGoClient Client { get; set; }
        public IDictionary<string, StringValues> DefaultQuery { get; } = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
        public IDictionary<string, StringValues> DefaultHeaders { get; } = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
        public MethodDescriptor Descriptor { get; set; }
        public Func<IRetryer> RetryerFactory { get; set; }
    }
}