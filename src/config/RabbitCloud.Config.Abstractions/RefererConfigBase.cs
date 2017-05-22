namespace RabbitCloud.Config.Abstractions
{
    public class RefererConfigBase
    {
        public string Id { get; set; }

        /// <summary>
        /// 服务组。
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// 调用协议。
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// 注册中心。
        /// </summary>
        public string Registry { get; set; }
    }
}