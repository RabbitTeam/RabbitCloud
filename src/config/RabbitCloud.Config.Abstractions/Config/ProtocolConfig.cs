namespace RabbitCloud.Config.Abstractions.Config
{
    public class ProtocolConfig : Config
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }
}