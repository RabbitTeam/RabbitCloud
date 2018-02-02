using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabbit.Go.Formatters
{
    public interface IKeyValueFormatterFactory
    {
        IKeyValueFormatter CreateFormatter(Type type);
    }

    public static class KeyValueFormatterFactoryExtensions
    {
        public static async Task<IDictionary<string, string>> FormatAsync(this IKeyValueFormatterFactory keyValueFormatterFactory, IKeyValueFormatter formatter, object model, Type type = null, string name = null)
        {
            var context = new KeyValueFormatterContext(keyValueFormatterFactory)
            {
                BinderModelName = name ?? string.Empty,
                Model = model,
                ModelType = type ?? model.GetType()
            };
            await formatter.FormatAsync(context);

            return context.Result;
        }
    }
}