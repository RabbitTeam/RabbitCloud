using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Abstractions.Utilities
{
    public static class ReflectionUtilities
    {
        public static Type GetRealReturnType(this MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            return GetRealType(method.ReturnType);
        }

        public static Type GetRealType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsGenericType && typeof(Task).IsAssignableFrom(type))
            {
                return type.GenericTypeArguments[0];
            }
            return type;
        }

        public static T GetTypeAttribute<T>(this MemberInfo memberInfo)
        {
            return memberInfo.GetTypeAttributes<T>().FirstOrDefault();
        }

        public static IEnumerable<T> GetTypeAttributes<T>(this MemberInfo memberInfo)
        {
            if (memberInfo == null)
                throw new ArgumentNullException(nameof(memberInfo));

            return memberInfo.GetCustomAttributes().OfType<T>();
        }
    }
}