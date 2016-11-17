namespace RabbitCloud.Config.Abstractions.Config
{
    public class ReferenceConfig : Config
    {
        public RegistryConfig RegistryConfig { get; set; }
        public ProtocolConfig ProtocolConfig { get; set; }
        public string InterfaceType { get; set; }
    }
}