using System;

namespace Rabbit.Cloud.Abstractions
{
    public enum RabbitRpcExceptionCode
    {
        Unknown,
        Network,
        Timeout,
        Business,
        Forbidden
    }

    public class RabbitRpcException : RabbitException
    {
        public RabbitRpcException()
        {
        }

        public RabbitRpcException(int rabbitStatusCode, string realStatus, string message, Exception innerException) : base(message, innerException)
        {
            RabbitStatusCode = rabbitStatusCode;
            RealStatus = realStatus;
        }

        public RabbitRpcException(RabbitRpcExceptionCode rabbitStatusCode, string message) : this(rabbitStatusCode, null, message, null)
        {
        }

        public RabbitRpcException(RabbitRpcExceptionCode rabbitStatusCode, string realStatus, string message, Exception innerException) : this((int)rabbitStatusCode, realStatus, message, innerException)
        {
        }

        public int RabbitStatusCode { get; set; }

        public string RealStatus { get; set; }
    }

    public static class RabbitRpcExceptionExtensions
    {
        public static RabbitRpcExceptionCode GetRabbitRpcExceptionCode(this RabbitRpcException exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return Enum.TryParse<RabbitRpcExceptionCode>(exception.RabbitStatusCode.ToString(), out var value) ? value : RabbitRpcExceptionCode.Unknown;
        }

        public static bool IsBusiness(this RabbitRpcException exception)
        {
            return exception.RabbitStatusCode == (int)RabbitRpcExceptionCode.Business;
        }
    }
}