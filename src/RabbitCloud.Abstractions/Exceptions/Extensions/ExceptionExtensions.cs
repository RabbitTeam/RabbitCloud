using System;

namespace RabbitCloud.Abstractions.Exceptions.Extensions
{
    public static class ExceptionExtensions
    {
        public static bool IsBusinessException(this Exception exception)
        {
            return exception is RabbitBusinessException;
        }

        public static bool IsRabbitException(this Exception exception)
        {
            return exception is RabbitException;
        }
    }
}