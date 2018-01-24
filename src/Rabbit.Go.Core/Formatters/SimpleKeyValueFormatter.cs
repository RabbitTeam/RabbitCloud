using System;
using System.Threading.Tasks;

namespace Rabbit.Go.Formatters
{
    public class SimpleKeyValueFormatter : IKeyValueFormatter
    {
        #region Implementation of IKeyValueFormatter

        public Task FormatAsync(KeyValueFormatterContext context)
        {
            var key = context.BinderModelName;
            var model = context.Model;

            if (model == null)
                return Task.CompletedTask;

            string value = null;

            switch (Type.GetTypeCode(context.ModelType))
            {
                case TypeCode.Boolean:
                    value = model is bool b && b ? "true" : "false";
                    break;

                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.String:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    value = model.ToString();
                    break;

                case TypeCode.DateTime:
                    value = model is DateTime dateTime ? dateTime.ToString("yyyy-MM-dd HH:mm:ss") : null;
                    break;

                case TypeCode.DBNull:
                    value = null;
                    break;

                case TypeCode.Empty:
                    value = string.Empty;
                    break;

                case TypeCode.Object:
                    break;

                default:
                    return Task.CompletedTask;
            }

            context.Result[key] = value;
            return Task.CompletedTask;
        }

        #endregion Implementation of IKeyValueFormatter
    }
}