namespace RabbitCloud.Rpc.Abstractions.Features
{
    /// <summary>
    /// 一个抽象的Rpc响应特性。
    /// </summary>
    public interface IRpcResponseFeature
    {
        /// <summary>
        /// 响应主体。
        /// </summary>
        object Body { get; set; }
    }

    public class RpcResponseFeature : IRpcResponseFeature
    {
        #region Implementation of IRpcResponseFeature

        /// <summary>
        /// 响应主体。
        /// </summary>
        public object Body { get; set; }

        #endregion Implementation of IRpcResponseFeature
    }
}