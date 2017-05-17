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
    }
}