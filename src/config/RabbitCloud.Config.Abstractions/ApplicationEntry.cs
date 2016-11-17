namespace RabbitCloud.Config.Abstractions
{
    public class ApplicationEntry
    {
        public string Name { get; set; }
        public ReferenceEntry[] References { get; set; }
        public ServiceEntry[] Services { get; set; }
    }
}