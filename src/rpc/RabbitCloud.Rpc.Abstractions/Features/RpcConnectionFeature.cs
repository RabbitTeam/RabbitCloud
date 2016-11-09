using System.Net;

namespace RabbitCloud.Rpc.Abstractions.Features
{
    /// <summary>
    /// 一个抽象的Rpc连接特性。
    /// </summary>
    public interface IRpcConnectionFeature
    {
        /// <summary>
        /// 连接Id。
        /// </summary>
        string ConnectionId { get; set; }

        /// <summary>
        /// 本地Ip地址。
        /// </summary>
        IPAddress LocalIpAddress { get; set; }

        /// <summary>
        /// 本地端口。
        /// </summary>
        int LocalPort { get; set; }

        /// <summary>
        /// 远程Ip地址。
        /// </summary>
        IPAddress RemoteIpAddress { get; set; }

        /// <summary>
        /// 远程端口。
        /// </summary>
        int RemotePort { get; set; }
    }

    public class RpcConnectionFeature : IRpcConnectionFeature
    {
        #region Implementation of IRpcConnectionFeature

        /// <summary>
        /// 连接Id。
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// 本地Ip地址。
        /// </summary>
        public IPAddress LocalIpAddress { get; set; }

        /// <summary>
        /// 本地端口。
        /// </summary>
        public int LocalPort { get; set; }

        /// <summary>
        /// 远程Ip地址。
        /// </summary>
        public IPAddress RemoteIpAddress { get; set; }

        /// <summary>
        /// 远程端口。
        /// </summary>
        public int RemotePort { get; set; }

        #endregion Implementation of IRpcConnectionFeature
    }
}