namespace RabbitCloud.Config.Abstractions
{
    public class ServiceConfig : ServiceConfigBase
    {
        /// <summary>
        /// 服务接口类型名称
        /// </summary>
        public string Interface { get; set; }

        /// <summary>
        /// 服务实现类型名称
        /// </summary>
        public string Implement { get; set; }
    }
}