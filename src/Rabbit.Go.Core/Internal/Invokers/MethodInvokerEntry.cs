using Rabbit.Go.Abstractions;
using Rabbit.Go.Abstractions.Codec;
using Rabbit.Go.Formatters;
using Rabbit.Go.Interceptors;
using Rabbit.Go.Internal;
using System.Collections.Generic;

namespace Rabbit.Go.Core.Internal
{
    public class MethodInvokerEntry
    {
        public IGoClient Client { get; set; }
        public ICodec Codec { get; set; }
        public IKeyValueFormatterFactory KeyValueFormatterFactory { get; set; }
        public ITemplateParser TemplateParser { get; set; }
        public IList<IInterceptorMetadata> Interceptors { get; set; }
    }
}