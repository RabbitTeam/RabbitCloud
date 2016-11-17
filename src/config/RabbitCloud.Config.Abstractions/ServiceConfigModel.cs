using System;

namespace RabbitCloud.Config.Abstractions
{
    public class ServiceConfigModel
    {
        public string ServiceId { get; set; }
        public Func<object> ServiceFactory { get; set; }
        public Type ServiceType { get; set; }
    }
}