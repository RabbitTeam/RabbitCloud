using Grpc.Core;
using Rabbit.Cloud.Abstractions;

namespace Rabbit.Cloud.Grpc.Abstractions.Utilities.Extensions
{
    public static class RpcExceptionExtensions
    {
        public static RabbitRpcException WrapRabbitRpcException(this RpcException rpcException)
        {
            var code = GetRabbitRpcExceptionCode(rpcException.Status.StatusCode);
            return new RabbitRpcException(code, rpcException.Status.Detail, null, rpcException);
        }

        public static RabbitRpcExceptionCode GetRabbitRpcExceptionCode(StatusCode statusCode)
        {
            switch (statusCode)
            {
                case StatusCode.OK:
                    throw new RabbitException("Unable to get exception status code from successful status code.");

                case StatusCode.Cancelled:
                case StatusCode.InvalidArgument:
                case StatusCode.PermissionDenied:
                case StatusCode.Unauthenticated:
                    return RabbitRpcExceptionCode.Forbidden;

                case StatusCode.DeadlineExceeded:
                    return RabbitRpcExceptionCode.Timeout;

                case StatusCode.Internal:
                case StatusCode.Unknown:
                case StatusCode.Unimplemented:
                case StatusCode.ResourceExhausted:
                case StatusCode.NotFound:
                    return RabbitRpcExceptionCode.Unknown;

                case StatusCode.OutOfRange:
                case StatusCode.Unavailable:
                case StatusCode.Aborted:
                case StatusCode.DataLoss:
                case StatusCode.AlreadyExists:
                case StatusCode.FailedPrecondition:
                    return RabbitRpcExceptionCode.Network;

                default:
                    return RabbitRpcExceptionCode.Unknown;
            }
        }
    }
}