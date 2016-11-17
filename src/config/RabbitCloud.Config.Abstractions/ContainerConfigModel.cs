using RabbitCloud.Abstractions;
using System.Collections.Generic;

namespace RabbitCloud.Config.Abstractions
{
    public class ContainerConfigModel
    {
        public Url Address { get; set; }

        public IEnumerable<ServiceConfigModel> ServiceConfigModels { get; set; }
    }
}