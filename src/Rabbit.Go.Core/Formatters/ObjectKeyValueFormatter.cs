using Rabbit.Go.Utilities;
using System;
using System.Threading.Tasks;

namespace Rabbit.Go.Formatters
{
    public class ObjectKeyValueFormatter : IKeyValueFormatter
    {
        #region Implementation of IKeyValueFormatter

        public async Task FormatAsync(KeyValueFormatterContext context)
        {
            var code = Type.GetTypeCode(context.ModelType);

            if (code != TypeCode.Object)
                return;

            var properties = context.ModelType.GetProperties();

            foreach (var property in properties)
            {
                if (property.GetTypeAttribute<PropertyIgnoreAttribute>() != null)
                    continue;
                var propertyContext = new KeyValueFormatterContext(context.FormatterFactory)
                {
                    BinderModelName = context.BinderModelName + "." + (property.GetTypeAttribute<KeyNameAttribute>()?.Name ?? property.Name),
                    ModelType = property.PropertyType,
                    Model = property.GetValue(context.Model)
                };

                var customKeyValueFormatter = property.GetTypeAttribute<IKeyValueFormatter>();

                var keyValueFormatter = customKeyValueFormatter ?? context.FormatterFactory.CreateFormatter(propertyContext.ModelType);
                await keyValueFormatter.FormatAsync(propertyContext);

                foreach (var t in propertyContext.Result)
                    context.Result[t.Key] = t.Value;
            }
        }

        #endregion Implementation of IKeyValueFormatter
    }
}