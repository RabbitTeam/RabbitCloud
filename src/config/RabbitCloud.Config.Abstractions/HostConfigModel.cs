using System.Collections.Generic;
using System.Net;

namespace RabbitCloud.Config.Abstractions
{
    public class HostConfigModel
    {
        public string Protocol { get; set; }
        public EndPoint Address { get; set; }

        public IEnumerable<ServiceConfigModel> ServiceConfigModels { get; set; }
    }
}