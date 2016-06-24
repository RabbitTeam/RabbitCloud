using System;
using System.Collections.Generic;

namespace Rabbit.Rpc.Messages
{
    /// <summary>
    /// 远程调用消息。
    /// </summary>
    public class RemoteInvokeMessage : TransportMessage
    {
        public RemoteInvokeMessage()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// 服务Id。
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        /// 服务参数。
        /// </summary>
        public IDictionary<string, object> Parameters { get; set; }
    }
}