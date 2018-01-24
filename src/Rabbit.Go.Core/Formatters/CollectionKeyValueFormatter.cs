using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Go.Formatters
{
    public class CollectionKeyValueFormatter : IKeyValueFormatter
    {
        #region Implementation of IKeyValueFormatter

        public async Task FormatAsync(KeyValueFormatterContext context)
        {
            var model = context.Model;
            if (model == null)
                return;

            var modelType = context.ModelType;

            Type elementType;
            if (modelType.HasElementType)
            {
                elementType = modelType.GetElementType();
            }
            else if (modelType.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(modelType.GetGenericTypeDefinition()))
            {
                elementType = modelType.GenericTypeArguments[0];
            }
            else
            {
                return;
            }

            var keyValueFormatter = context.FormatterFactory.CreateFormatter(elementType);

            if (model is IEnumerable enumerable)
            {
                var i = 0;
                foreach (var item in enumerable.Cast<object>())
                {
                    var itemContext = new KeyValueFormatterContext(context.FormatterFactory)
                    {
                        BinderModelName = context.BinderModelName + $"[{i}]",
                        Model = item,
                        ModelType = elementType
                    };
                    await keyValueFormatter.FormatAsync(itemContext);

                    foreach (var t in itemContext.Result)
                        context.Result[t.Key] = t.Value;

                    i++;
                }
            }
        }

        #endregion Implementation of IKeyValueFormatter
    }
}