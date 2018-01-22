using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabbit.Go.Core.Formatters
{
    public interface IKeyValueFormatterFactory
    {
        IKeyValueFormatter CreateFormatter(Type type);
    }

    public static class KeyValueFormatterFactoryExtensions
    {
        public static async Task<IDictionary<string, string>> FormatAsync(this IKeyValueFormatterFactory keyValueFormatterFactory, object model, Type type = null, string name = null)
        {
            var modelType = type ?? model.GetType();
            var context = new KeyValueFormatterContext(keyValueFormatterFactory)
            {
                BinderModelName = name ?? string.Empty,
                Model = model,
                ModelType = type
            };
            await keyValueFormatterFactory.CreateFormatter(modelType).FormatAsync(context);

            return context.Result;
        }
    }
}