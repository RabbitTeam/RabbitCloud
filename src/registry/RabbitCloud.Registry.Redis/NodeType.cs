namespace RabbitCloud.Registry.Redis
{
    /// <summary>
    /// 节点类型。
    /// </summary>
    internal enum NodeType
    {
        /// <summary>
        /// 可用的服务器。
        /// </summary>
        AvailableServer,

        /// <summary>
        /// 不可用的服务器。
        /// </summary>
        UnavailableServer,

        /// <summary>
        /// 客户端。
        /// </summary>
        Client
    }
}