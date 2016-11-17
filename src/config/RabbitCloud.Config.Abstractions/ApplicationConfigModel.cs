using RabbitCloud.Registry.Abstractions;
using RabbitCloud.Rpc.Abstractions.Protocol;
using RabbitCloud.Rpc.Abstractions.Proxy;

namespace RabbitCloud.Config.Abstractions
{
    public class ApplicationConfigModel
    {
        public ContainerConfigModel[] Containers { get; set; }
        public IProxyFactory ProxyFactory { get; set; }
        public IRegistry Registry { get; set; }
        public IProtocol Protocol { get; set; }
    }
}