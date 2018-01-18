using Castle.DynamicProxy;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rabbit.Go.Utilities
{
    public static class InvocationExtensions
    {
        public static IDictionary<string, object> MappingArguments(this IInvocation invocation)
        {
            return GetArguments(invocation.Method, invocation.Arguments);
        }

        private static IDictionary<string, object> GetArguments(MethodBase method, IReadOnlyList<object> arguments)
        {
            var parameters = method.GetParameters();
            if (!parameters.Any())
                return null;

            var dictionary = new Dictionary<string, object>(parameters.Length);
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var value = arguments[i];
                dictionary[parameter.Name] = value;
            }
            return dictionary;
        }
    }
}