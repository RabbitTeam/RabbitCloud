using System;

namespace RabbitCloud.Rpc.Abstractions.Exceptions
{
    /// <summary>
    /// Rpc异常。
    /// </summary>
    public class RpcException : Exception
    {
        public RpcException()
        {
        }

        public RpcException(string message) : base(message)
        {
        }
    }
}