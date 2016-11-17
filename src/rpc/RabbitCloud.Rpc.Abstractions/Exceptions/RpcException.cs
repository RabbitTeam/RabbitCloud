using System;

namespace RabbitCloud.Rpc.Abstractions.Exceptions
{
    /// <summary>
    /// Rpc异常。
    /// </summary>
    public class RpcException : Exception
    {
        /// <summary>
        /// 异常代码。
        /// </summary>
        public int Code { get; set; }

        #region Constructor

        public RpcException() : this(RpcExceptionConstants.Unknown)
        {
        }

        public RpcException(int code) : this(code, null)
        {
        }

        public RpcException(string message) : this(RpcExceptionConstants.Unknown, message)
        {
        }

        public RpcException(int code, string message) : this(code, message, null)
        {
        }

        public RpcException(int code, string message, Exception innerException) : base(message, innerException)
        {
            Code = code;
        }

        #endregion Constructor
    }

    /// <summary>
    /// Rpc网络异常。
    /// </summary>
    public class RpcNetworkException : RpcException
    {
        public RpcNetworkException(string message)
            : this(message, null)
        {
        }

        public RpcNetworkException(string message, Exception innerException)
            : base(RpcExceptionConstants.Network, message, innerException)
        {
        }
    }

    /// <summary>
    /// Rpc超时异常。
    /// </summary>
    public class RpcTimeoutException : RpcException
    {
        public RpcTimeoutException(string message)
            : this(message, null)
        {
        }

        public RpcTimeoutException(string message, Exception innerException)
            : base(RpcExceptionConstants.Timeout, message, innerException)
        {
        }
    }

    /// <summary>
    /// Rpc业务异常。
    /// </summary>
    public class RpcBusinessException : RpcException
    {
        public RpcBusinessException(string message)
            : this(message, null)
        {
        }

        public RpcBusinessException(string message, Exception innerException)
            : base(RpcExceptionConstants.Business, message, innerException)
        {
        }
    }

    /// <summary>
    /// Rpc序列化异常。
    /// </summary>
    public class RpcSerializationException : RpcException
    {
        public RpcSerializationException(string message)
            : this(message, null)
        {
        }

        public RpcSerializationException(string message, Exception innerException)
            : base(RpcExceptionConstants.Serialization, message, innerException)
        {
        }
    }

    /// <summary>
    /// Rpc异常扩展方法。
    /// </summary>
    public static class RpcExceptionExtensions
    {
        /// <summary>
        /// 判断一个rpc异常是否为网络异常。
        /// </summary>
        /// <param name="exception">rpc异常。</param>
        /// <returns>如果是网络异常则返回true，否则返回false。</returns>
        public static bool IsNetwork(this RpcException exception)
        {
            return exception.Code == RpcExceptionConstants.Network;
        }

        /// <summary>
        /// 判断一个rpc异常是否为超时异常。
        /// </summary>
        /// <param name="exception">rpc异常。</param>
        /// <returns>如果是超时异常则返回true，否则返回false。</returns>
        public static bool IsTimeout(this RpcException exception)
        {
            return exception.Code == RpcExceptionConstants.Timeout;
        }

        /// <summary>
        /// 判断一个rpc异常是否为业务异常。
        /// </summary>
        /// <param name="exception">rpc异常。</param>
        /// <returns>如果是业务异常则返回true，否则返回false。</returns>
        public static bool IsBusiness(this RpcException exception)
        {
            return exception.Code == RpcExceptionConstants.Business;
        }

        /// <summary>
        /// 判断一个rpc异常是否为序列化异常。
        /// </summary>
        /// <param name="exception">rpc异常。</param>
        /// <returns>如果是序列化异常则返回true，否则返回false。</returns>
        public static bool IsSerialization(this RpcException exception)
        {
            return exception.Code == RpcExceptionConstants.Serialization;
        }
    }
}