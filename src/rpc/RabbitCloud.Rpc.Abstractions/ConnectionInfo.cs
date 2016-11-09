using RabbitCloud.Rpc.Abstractions.Features;
using System.Net;

namespace RabbitCloud.Rpc.Abstractions
{
    /// <summary>
    /// 一个抽象的连接信息。
    /// </summary>
    public abstract class ConnectionInfo
    {
        /// <summary>
        /// 远程Ip地址。
        /// </summary>
        public abstract IPAddress RemoteIpAddress { get; set; }

        /// <summary>
        /// 远程端口。
        /// </summary>
        public abstract int RemotePort { get; set; }

        /// <summary>
        /// 本地Ip地址。
        /// </summary>
        public abstract IPAddress LocalIpAddress { get; set; }

        /// <summary>
        /// 本地端口。
        /// </summary>
        public abstract int LocalPort { get; set; }
    }

    public class DefaultConnectionInfo : ConnectionInfo
    {
        private readonly IRpcConnectionFeature _connectionFeature;

        public DefaultConnectionInfo(IRpcFeatureCollection features)
        {
            _connectionFeature = features.Get<IRpcConnectionFeature>();
        }

        #region Overrides of ConnectionInfo

        /// <summary>
        /// 远程Ip地址。
        /// </summary>
        public override IPAddress RemoteIpAddress
        {
            get { return _connectionFeature.RemoteIpAddress; }
            set { _connectionFeature.RemoteIpAddress = value; }
        }

        /// <summary>
        /// 远程端口。
        /// </summary>
        public override int RemotePort
        {
            get { return _connectionFeature.RemotePort; }
            set { _connectionFeature.RemotePort = value; }
        }

        /// <summary>
        /// 本地Ip地址。
        /// </summary>
        public override IPAddress LocalIpAddress
        {
            get { return _connectionFeature.LocalIpAddress; }
            set { _connectionFeature.LocalIpAddress = value; }
        }

        /// <summary>
        /// 本地端口。
        /// </summary>
        public override int LocalPort
        {
            get { return _connectionFeature.LocalPort; }
            set { _connectionFeature.LocalPort = value; }
        }

        #endregion Overrides of ConnectionInfo
    }
}