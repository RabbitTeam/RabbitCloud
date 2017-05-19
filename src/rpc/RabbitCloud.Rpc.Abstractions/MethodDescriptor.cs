using RabbitCloud.Abstractions.Utilities;
using System.Reflection;

namespace RabbitCloud.Rpc.Abstractions
{
    /// <summary>
    /// 方法描述符。
    /// </summary>
    public struct MethodDescriptor
    {
        public MethodDescriptor(MethodInfo method)
        {
            InterfaceName = method.DeclaringType.Name;
            MethodName = method.Name;
            ParamtersSignature = ReflectUtil.GetMethodParamDesc(method);
        }

        /// <summary>
        /// 类型名称。
        /// </summary>
        public string InterfaceName { get; set; }

        /// <summary>
        /// 方法名。
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// 方法参数签名。
        /// </summary>
        public string ParamtersSignature { get; set; }
    }
}