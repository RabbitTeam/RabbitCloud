using System.Collections.Specialized;

namespace RabbitCloud.Rpc.Abstractions
{
    /// <summary>
    /// 一个抽象的RPC请求。
    /// </summary>
    public interface IRequest
    {
        /// <summary>
        /// 请求Id。
        /// </summary>
        long RequestId { get; }

        /// <summary>
        /// 接口名称。
        /// </summary>
        string InterfaceName { get; }

        /// <summary>
        /// 方法名称。
        /// </summary>
        string MethodName { get; }

        /// <summary>
        /// 参数类型。
        /// </summary>
        string[] ParamtersType { get; }

        /// <summary>
        /// 服务参数。
        /// </summary>
        object[] Arguments { get; }

        /// <summary>
        /// 获取请求参数。
        /// </summary>
        /// <returns>键值对应数组。</returns>
        NameValueCollection GetParameters();

        /// <summary>
        /// 设置一个请求参数。
        /// </summary>
        /// <param name="name">参数名称。</param>
        /// <param name="value">参数值。</param>
        void SetParameter(string name, string value);

        /// <summary>
        /// 获取一个请求参数。
        /// </summary>
        /// <param name="name">参数名称。</param>
        /// <returns>参数值。</returns>
        string GetParameter(string name);
    }
}