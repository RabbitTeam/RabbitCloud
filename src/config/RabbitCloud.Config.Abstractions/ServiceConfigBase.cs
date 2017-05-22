namespace RabbitCloud.Config.Abstractions
{
    public class ServiceConfigBase
    {
        public string Id { get; set; }

        /// <summary>
        /// 服务组
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// 服务导出信息
        /// </summary>
        public string Export { get; set; }

        /// <summary>
        /// 服务注册信息。
        /// </summary>
        public string Registry { get; set; }
    }
}