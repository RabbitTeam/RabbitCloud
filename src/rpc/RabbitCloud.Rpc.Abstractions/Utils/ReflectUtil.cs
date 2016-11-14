using System.Linq;
using System.Reflection;
using System.Text;

namespace RabbitCloud.Rpc.Abstractions.Utils
{
    public class ReflectUtil
    {
        public static string GetMethodSignature(MethodInfo method)
        {
            return GetMethodSignature(method.Name, method.GetParameters().Select(i => i.ParameterType.FullName).ToArray());
        }

        public static string GetMethodSignature(string methodName, string[] parametersType)
        {
            var builder = new StringBuilder($"{methodName}(");

            var paramtersType = parametersType;
            if (paramtersType == null || !paramtersType.Any())
            {
                builder.Append(")");
                return builder.ToString();
            }

            builder.Append(string.Join(",", paramtersType));
            builder.Append(")");
            return builder.ToString();
        }
    }
}