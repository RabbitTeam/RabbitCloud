using Grpc.Core;
using Rabbit.Cloud.Abstractions;
using System;

namespace Rabbit.Cloud.Client.Grpc.Utilities.Extensions
{
    public static class RpcExceptionExtensions
    {
        public static int GetStatusCode(StatusCode statusCode)
        {
            switch (statusCode)
            {
                case StatusCode.OK:
                    throw new RabbitException("Unable to get exception status code from successful status code.");

                case StatusCode.Aborted:
                case StatusCode.Cancelled:
                    return 400;

                case StatusCode.InvalidArgument:
                    return 412;

                case StatusCode.PermissionDenied:
                    return 403;

                case StatusCode.Unauthenticated:
                    return 401;

                case StatusCode.NotFound:
                    return 404;

                case StatusCode.OutOfRange:
                    return 416;

                case StatusCode.Unavailable:
                case StatusCode.ResourceExhausted:
                case StatusCode.DeadlineExceeded:
                    return 503;

                case StatusCode.Internal:
                case StatusCode.Unknown:
                case StatusCode.Unimplemented:
                case StatusCode.DataLoss:
                case StatusCode.AlreadyExists:
                case StatusCode.FailedPrecondition:
                    return 500;

                default:
                    throw new ArgumentOutOfRangeException(nameof(statusCode), statusCode, null);
            }
        }
    }
}