namespace RabbitCloud.Rpc.Abstractions.Features
{
    /// <summary>
    /// Rpc请求特性。
    /// </summary>
    public interface IRpcRequestFeature
    {
        /// <summary>
        /// 服务Id。
        /// </summary>
        string ServiceId { get; set; }

        /// <summary>
        /// 请求主体。
        /// </summary>
        object Body { get; set; }
    }

    public class RpcRequestFeature : IRpcRequestFeature
    {
        #region Implementation of IRpcRequestFeature

        /// <summary>
        /// 服务Id。
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        /// 请求主体。
        /// </summary>
        public object Body { get; set; }

        #endregion Implementation of IRpcRequestFeature
    }
}