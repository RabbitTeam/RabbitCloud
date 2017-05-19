using RabbitCloud.Abstractions;

namespace RabbitCloud.Registry.Abstractions
{
    public struct ServiceRegistryDescriptor
    {
        public string Protocol { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public ServiceKey ServiceKey { get; set; }
    }
}