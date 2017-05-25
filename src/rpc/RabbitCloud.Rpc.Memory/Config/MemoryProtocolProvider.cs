using RabbitCloud.Config.Abstractions;
using RabbitCloud.Config.Abstractions.Adapter;
using RabbitCloud.Rpc.Abstractions;
using System.Collections.Concurrent;

namespace RabbitCloud.Rpc.Memory.Config
{
    public class MemoryProtocolProvider : IProtocolProvider
    {
        private static readonly ConcurrentDictionary<string, MemoryProtocol> MemoryProtocols = new ConcurrentDictionary<string, MemoryProtocol>();

        #region Implementation of IProtocolProvider

        public string Name { get; } = "Memory";

        public IProtocol CreateProtocol(ProtocolConfig config)
        {
            return MemoryProtocols.GetOrAdd(config.Id, k => new MemoryProtocol());
        }

        #endregion Implementation of IProtocolProvider
    }
}