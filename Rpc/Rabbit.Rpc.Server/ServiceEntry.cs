using System;
using System.Collections.Generic;

namespace Rabbit.Rpc.Server
{
    /// <summary>
    /// 服务条目。
    /// </summary>
    public class ServiceEntry
    {
        /// <summary>
        /// 执行委托。
        /// </summary>
        public Func<IDictionary<string, object>, object> Func { get; set; }

        /// <summary>
        /// 服务描述符。
        /// </summary>
        public ServiceDescriptor Descriptor { get; set; }
    }
}