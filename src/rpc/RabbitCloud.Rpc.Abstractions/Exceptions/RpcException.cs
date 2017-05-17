using System;

namespace RabbitCloud.Rpc.Abstractions.Exceptions
{
    public class RpcException : Exception
    {
        public RpcException()
        {
        }

        public RpcException(string message) : base(message)
        {
        }

        public RpcException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}