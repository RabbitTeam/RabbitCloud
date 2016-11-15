namespace RabbitCloud.Registry.Redis
{
    public class RedisConnectionInfo
    {
        public string ConnectionString { get; set; }
        public int Database { get; set; }
        public string ApplicationId { get; set; }
    }
}