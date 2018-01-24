using System;
using System.Collections.Generic;

namespace Rabbit.Go.Formatters
{
    public static class TypeExtensions
    {
        public static bool IsCollection(this Type type)
        {
            return type.HasElementType || type.IsGenericType &&
                   typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition());
        }
    }
}