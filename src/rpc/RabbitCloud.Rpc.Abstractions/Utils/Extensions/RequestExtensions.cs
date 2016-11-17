namespace RabbitCloud.Rpc.Abstractions.Utils.Extensions
{
    public static class RequestExtensions
    {
        public static string GetMethodSignature(this IRequest request)
        {
            return ReflectUtil.GetMethodSignature(request.MethodName, request.ParamtersType);
        }

        public static string GetServiceKey(this IRequest request)
        {
            return ServiceKeyUtil.GetServiceKey(request.InterfaceName);
        }
    }
}