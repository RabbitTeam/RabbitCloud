namespace RabbitCloud.Config.Abstractions.Config
{
    public class ApplicationConfig : Config
    {
        public string Name { get; set; }
        public ServiceExportConfig[] ServiceExportConfigs { get; set; }
        public ReferenceConfig[] ReferenceConfigs { get; set; }
    }
}