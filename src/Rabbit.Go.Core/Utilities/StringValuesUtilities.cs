using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace Rabbit.Go.Utilities
{
    public static class StringValuesExtensions
    {
        public static IDictionary<string, StringValues> Merge(this IDictionary<string, StringValues> source, IDictionary<string, string> target)
        {
            foreach (var item in target)
            {
                if (!source.TryGetValue(item.Key, out var value))
                    value = item.Value;
                else
                    value = StringValues.Concat(value, item.Value);

                source[item.Key] = value;
            }

            return source;
        }
    }
}