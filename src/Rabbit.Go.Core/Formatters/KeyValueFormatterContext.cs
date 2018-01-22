using System;
using System.Collections.Generic;

namespace Rabbit.Go.Core.Formatters
{
    public class KeyValueFormatterContext
    {
        public KeyValueFormatterContext(IKeyValueFormatterFactory formatterFactory)
        {
            FormatterFactory = formatterFactory;
        }

        public IKeyValueFormatterFactory FormatterFactory { get; }
        public object Model { get; set; }
        public virtual Type ModelType { get; set; }
        public string BinderModelName { get; set; }
        public IDictionary<string, string> Result { get; } = new Dictionary<string, string>();
    }
}