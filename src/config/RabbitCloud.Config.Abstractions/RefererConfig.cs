namespace RabbitCloud.Config.Abstractions
{
    public class RefererConfig : RefererConfigBase
    {
        /// <summary>
        /// 服务接口类型名称。
        /// </summary>
        public string Interface { get; set; }

        /// <summary>
        /// 服务主机。
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 服务端口。
        /// </summary>
        public int Port { get; set; }
    }
}