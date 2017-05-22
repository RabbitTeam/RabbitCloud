namespace RabbitCloud.Config.Abstractions
{
    public class CloudApplicationModel
    {
        public ProtocolConfig[] Protocols { get; set; }
        public ServiceConfig[] Services { get; set; }
        public RefererConfig[] Referers { get; set; }
        public RegistryConfig[] Registrys { get; set; }
    }
}