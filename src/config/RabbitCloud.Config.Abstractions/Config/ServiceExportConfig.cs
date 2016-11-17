namespace RabbitCloud.Config.Abstractions.Config
{
    public class ServiceExportConfig : Config
    {
        public RegistryConfig RegistryConfig { get; set; }
        public ProtocolConfig ProtocolConfig { get; set; }
        public ServiceConfig ServiceConfig { get; set; }
    }
}