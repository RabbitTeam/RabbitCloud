using System;
using System.Collections.Generic;

namespace RabbitCloud.Rpc.Abstractions.Features
{
    /// <summary>
    /// 一个抽象的Rpc响应特性。
    /// </summary>
    public interface IRpcResponseFeature
    {
        /// <summary>
        /// 请求头。
        /// </summary>
        IDictionary<string, string> Headers { get; set; }

        /// <summary>
        /// 响应主体。
        /// </summary>
        object Body { get; set; }
    }

    public class RpcResponseFeature : IRpcResponseFeature
    {
        public RpcResponseFeature()
        {
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        #region Implementation of IRpcResponseFeature

        /// <summary>
        /// 请求头。
        /// </summary>
        public IDictionary<string, string> Headers { get; set; }

        /// <summary>
        /// 响应主体。
        /// </summary>
        public object Body { get; set; }

        #endregion Implementation of IRpcResponseFeature
    }
}