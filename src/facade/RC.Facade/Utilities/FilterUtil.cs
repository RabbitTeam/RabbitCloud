using Rabbit.Cloud.Facade.Abstractions.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rabbit.Cloud.Facade.Utilities
{
    internal static class FilterUtil
    {
        public static IEnumerable<T> GetFilters<T>(this MethodInfo method, IServiceProvider services) where T : IFilterMetadata
        {
            return GetFilters<T>(method.GetCustomAttributes(false));
        }

        public static IEnumerable<T> GetFilters<T>(object[] attributes) where T : IFilterMetadata
        {
            return attributes.OfType<IFilterMetadata>().Cast<T>();
        }
    }
}