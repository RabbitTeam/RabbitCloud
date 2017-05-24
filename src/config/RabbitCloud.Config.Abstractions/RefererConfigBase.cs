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

        /// <summary>
        /// 采用的集群策略。
        /// </summary>
        public string Cluster { get; set; }

        /// <summary>
        /// 采用的负载均衡策略。
        /// </summary>
        public string LoadBalance { get; set; }

        /// <summary>
        /// 采用的高可用策略。
        /// </summary>
        public string HaStrategy { get; set; }
    }
}