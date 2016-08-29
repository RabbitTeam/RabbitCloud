using RabbitCloud.Abstractions;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RabbitCloud.Rpc.Default.Service.Message
{
    public abstract class RpcMessage
    {
        public Id Id { get; set; }

        #region Implementation of IMetadataFeature

        /// <summary>
        /// 元数据。
        /// </summary>
        public IDictionary<string, object> Metadata { get; set; } = new ConcurrentDictionary<string, object>();

        #endregion Implementation of IMetadataFeature
    }
}