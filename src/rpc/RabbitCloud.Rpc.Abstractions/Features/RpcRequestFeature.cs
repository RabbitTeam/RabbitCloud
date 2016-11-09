using System;
using System.Collections.Generic;

namespace RabbitCloud.Rpc.Abstractions.Features
{
    /// <summary>
    /// Rpc请求特性。
    /// </summary>
    public interface IRpcRequestFeature
    {
        /// <summary>
        /// 请求主体。
        /// </summary>
        object Body { get; set; }

        /// <summary>
        /// 请求头。
        /// </summary>
        IDictionary<string, string> Headers { get; set; }

        /// <summary>
        /// 路径。
        /// </summary>
        string Path { get; set; }

        /// <summary>
        /// 基础路径。
        /// </summary>
        string PathBase { get; set; }

        /// <summary>
        /// 查询字符串。
        /// </summary>
        string QueryString { get; set; }

        /// <summary>
        /// 格式、协议。
        /// </summary>
        string Scheme { get; set; }
    }

    public class RpcRequestFeature : IRpcRequestFeature
    {
        public RpcRequestFeature()
        {
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Path = string.Empty;
            PathBase = string.Empty;
            QueryString = string.Empty;
            Scheme = string.Empty;
        }

        #region Implementation of IRpcRequestFeature

        /// <summary>
        /// 请求主体。
        /// </summary>
        public object Body { get; set; }

        /// <summary>
        /// 请求头。
        /// </summary>
        public IDictionary<string, string> Headers { get; set; }

        /// <summary>
        /// 路径。
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 基础路径。
        /// </summary>
        public string PathBase { get; set; }

        /// <summary>
        /// 查询字符串。
        /// </summary>
        public string QueryString { get; set; }

        /// <summary>
        /// 格式、协议。
        /// </summary>
        public string Scheme { get; set; }

        #endregion Implementation of IRpcRequestFeature
    }
}