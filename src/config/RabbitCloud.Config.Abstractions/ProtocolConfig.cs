namespace RabbitCloud.Config.Abstractions
{
    public class ProtocolConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string Formatter { get; set; }

        public string Cluster { get; set; }
        public string LoadBalance { get; set; }
        public string HaStrategy { get; set; }
    }
}