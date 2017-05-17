using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RabbitCloud.Abstractions.Utilities
{
    public class ReflectUtil
    {
        public static string GetMethodDesc(MethodInfo method)
        {
            var methodParamDesc = GetMethodParamDesc(method);
            return GetMethodDesc(method.Name, methodParamDesc);
        }

        public static string GetMethodDesc(string methodName, string paramDesc)
        {
            return paramDesc == null ? methodName + "()" : methodName + "(" + paramDesc + ")";
        }

        public static string GetMethodParamDesc(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            if (!parameters?.Any() ?? true)
                return null;

            var builder = new StringBuilder();
            foreach (var type in parameters.Select(i => i.ParameterType))
            {
                builder
                    .Append(GetName(type))
                    .Append(",");
            }

            return builder.ToString(0, builder.Length - 1);
        }

        public static string GetName(Type type)
        {
            if (!type.IsArray)
                return type.Name;

            var builder = new StringBuilder();
            builder.Append(type.Name);
            while (type.IsArray)
            {
                builder.Append("[]");
                type = type.GetElementType();
            }

            return builder.ToString();
        }
    }
}