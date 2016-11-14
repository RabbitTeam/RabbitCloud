using System;
using System.Collections.Specialized;

namespace RabbitCloud.Rpc.Abstractions
{
    /// <summary>
    /// 一个抽象的RPC响应。
    /// </summary>
    public interface IResponse
    {
        /// <summary>
        /// 请求Id。
        /// </summary>
        long RequestId { get; }

        /// <summary>
        /// 响应结果。
        /// </summary>
        object Result { get; }

        /// <summary>
        /// 响应结果中发生的异常。
        /// </summary>
        Exception Exception { get; }

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