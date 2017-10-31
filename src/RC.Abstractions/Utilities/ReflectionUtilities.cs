using System;
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

        private static Type GetRealType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsGenericType && typeof(Task).IsAssignableFrom(type))
            {
                return type.GenericTypeArguments[0];
            }
            return type;
        }
    }
}