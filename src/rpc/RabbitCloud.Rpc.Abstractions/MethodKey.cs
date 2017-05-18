using RabbitCloud.Abstractions.Utilities;
using System.Reflection;

namespace RabbitCloud.Rpc.Abstractions
{
    public struct MethodKey
    {
        public MethodKey(MethodInfo method)
        {
            Name = method.Name;
            ParamtersDesc = ReflectUtil.GetMethodParamDesc(method);
        }

        public string Name { get; set; }
        public string ParamtersDesc { get; set; }

        #region Equality members

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <returns>true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false. </returns>
        /// <param name="obj">The object to compare with the current instance. </param>
        public override bool Equals(object obj)
        {
            var key = (MethodKey)obj;
            return key.Name == Name && key.ParamtersDesc == ParamtersDesc;
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (ParamtersDesc != null ? ParamtersDesc.GetHashCode() : 0);
            }
        }

        #endregion Equality members
    }
}