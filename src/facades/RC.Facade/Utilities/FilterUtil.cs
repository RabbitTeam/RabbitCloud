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
            return GetFilters<T>(method.GetCustomAttributes(false), services);
        }

        public static IEnumerable<T> GetFilters<T>(object[] attributes, IServiceProvider services) where T : IFilterMetadata
        {
            var items1 = attributes.OfType<T>().Cast<IFilterMetadata>();
            var items2 = attributes.OfType<ServiceFilterAttribute>()
                .Where(i => typeof(T).IsAssignableFrom(i.ServiceType))
                .Select(i => i.CreateInstance(services));
            var items3 = attributes.OfType<TypeFilterAttribute>()
                .Where(i => typeof(T).IsAssignableFrom(i.ImplementationType))
                .Select(i => i.CreateInstance(services));

            return items1.Concat(items2).Concat(items3).Cast<T>();
        }
    }
}