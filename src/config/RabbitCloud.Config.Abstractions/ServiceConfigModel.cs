using System;

namespace RabbitCloud.Config.Abstractions
{
    public class ServiceConfigModel
    {
        public string ServiceKey { get; set; }
        public Func<object> ServiceFactory { get; set; }
        public Type ServiceType { get; set; }
    }
}