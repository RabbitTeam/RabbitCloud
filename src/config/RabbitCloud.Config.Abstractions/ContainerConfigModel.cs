using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions.Protocol;
using System.Collections.Generic;

namespace RabbitCloud.Config.Abstractions
{
    public class ContainerConfigModel
    {
        public IProtocol Protocol { get; set; }

        public Url Address { get; set; }

        public IEnumerable<ServiceConfigModel> ServiceConfigModels { get; set; }
    }
}