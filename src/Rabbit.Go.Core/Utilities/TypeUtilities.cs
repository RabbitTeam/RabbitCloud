using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rabbit.Go.Utilities
{
    public static class TypeUtilities
    {
        public static IEnumerable<T> GetTypeAttributes<T>(params MemberInfo[] members)
        {
            return members.SelectMany(m => m.GetCustomAttributes().OfType<T>());
        }

        public static IEnumerable<T> GetTypeAttributes<T>(this MemberInfo member)
        {
            return member.GetCustomAttributes().OfType<T>();
        }

        public static T GetTypeAttribute<T>(this MemberInfo member)
        {
            return member.GetTypeAttributes<T>().FirstOrDefault();
        }
    }
}