namespace RabbitCloud.Rpc.Abstractions.Exceptions
{
    /// <summary>
    /// RPC异常相关常量。
    /// </summary>
    public class RpcExceptionConstants
    {
        /// <summary>
        /// 未知异常代码。
        /// </summary>
        public const int Unknown = 0;

        /// <summary>
        /// 网络异常代码。
        /// </summary>
        public const int Network = 1;

        /// <summary>
        /// 超时异常代码。
        /// </summary>
        public const int Timeout = 2;

        /// <summary>
        /// 业务异常代码。
        /// </summary>
        public const int Business = 3;

        /// <summary>
        /// 序列化异常代码。
        /// </summary>
        public const int Serialization = 4;
    }
}