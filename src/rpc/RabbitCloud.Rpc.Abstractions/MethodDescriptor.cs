using RabbitCloud.Abstractions.Utilities;
using System.Reflection;

namespace RabbitCloud.Rpc.Abstractions
{
    /// <summary>
    /// 方法描述符。
    /// </summary>
    public struct MethodDescriptor
    {
        public static MethodDescriptor Empty;

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

        #region Overrides of ValueType

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <returns>true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false. </returns>
        /// <param name="obj">The object to compare with the current instance. </param>
        public override bool Equals(object obj)
        {
            var descriptor = (MethodDescriptor)obj;
            return string.Equals(InterfaceName, descriptor.InterfaceName) && string.Equals(MethodName, descriptor.MethodName) && string.Equals(ParamtersSignature, descriptor.ParamtersSignature);
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (InterfaceName != null ? InterfaceName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MethodName != null ? MethodName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ParamtersSignature != null ? ParamtersSignature.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(MethodDescriptor source, MethodDescriptor targer)
        {
            return source.Equals(targer);
        }

        public static bool operator !=(MethodDescriptor source, MethodDescriptor targer)
        {
            return !source.Equals(targer);
        }

        #endregion Overrides of ValueType
    }
}