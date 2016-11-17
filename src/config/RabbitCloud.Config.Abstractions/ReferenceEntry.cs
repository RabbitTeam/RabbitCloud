using RabbitCloud.Registry.Abstractions;
using RabbitCloud.Rpc.Abstractions.Protocol;

namespace RabbitCloud.Config.Abstractions
{
    public class ReferenceEntry
    {
        public IRegistry Registry { get; set; }
        public IProtocol Protocol { get; set; }
        public object ServiceProxy { get; set; }
    }
}