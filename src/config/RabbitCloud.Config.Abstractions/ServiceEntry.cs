using RabbitCloud.Registry.Abstractions;
using RabbitCloud.Rpc.Abstractions.Protocol;
using System;

namespace RabbitCloud.Config.Abstractions
{
    public class ServiceEntry
    {
        public IRegistry Registry { get; set; }
        public IProtocol Protocol { get; set; }
        public Type ServiceType { get; set; }
        public Func<object> ServiceFactory { get; set; }
    }
}