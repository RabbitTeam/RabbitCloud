using Rabbit.Cloud.Facade.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Rabbit.Cloud.Facade.MessageBuilding.Builders
{
    public static class MessageBuilderUtil
    {
        public static IEnumerable<KeyValuePair<string, string>> GetValues(ParameterDescriptor parameterDescriptor, object value)
        {
            var items = new List<KeyValuePair<string, string>>();

            AppendParameters(parameterDescriptor.BuildingInfo.BuildingModelName ?? parameterDescriptor.Name, value, items);

            return items;
        }

        private static void AppendParameters(string prefix, object value, ICollection<KeyValuePair<string, string>> items)
        {
            if (value == null)
                return;
            var valueType = value.GetType();
            switch (Type.GetTypeCode(valueType))
            {
                case TypeCode.Object:
                    if (value is IEnumerable enumerable)
                    {
                        var index = -1;
                        foreach (var item in enumerable)
                        {
                            index++;
                            AppendParameters($"{prefix}[{index}]", item, items);
                        }
                    }
                    else
                    {
                        foreach (var propertyInfo in valueType.GetProperties())
                        {
                            AppendParameters(prefix + "." + propertyInfo.Name, propertyInfo.GetValue(value), items);
                        }
                    }
                    break;

                default:
                    items.Add(new KeyValuePair<string, string>(prefix, value.ToString()));
                    break;
            }
        }
    }
}