using Rabbit.Go.Core.Utilities;
using System;

namespace Rabbit.Go.Core.Formatters
{
    public class KeyValueFormatterFactory : IKeyValueFormatterFactory
    {
        private static readonly IKeyValueFormatter CollectionKeyValueFormatter = new CollectionKeyValueFormatter();
        private static readonly IKeyValueFormatter ObjectKeyValueFormatter = new ObjectKeyValueFormatter();
        private static readonly IKeyValueFormatter SimpleKeyValueFormatter = new SimpleKeyValueFormatter();

        #region Implementation of IKeyValueFormatterFactory

        public IKeyValueFormatter CreateFormatter(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Object:
                    var customKeyValueFormatter = type.GetTypeAttribute<IKeyValueFormatter>();
                    if (customKeyValueFormatter != null)
                        return customKeyValueFormatter;
                    return type.IsCollection() ? CollectionKeyValueFormatter : ObjectKeyValueFormatter;

                default:
                    return SimpleKeyValueFormatter;
            }
        }

        #endregion Implementation of IKeyValueFormatterFactory
    }
}