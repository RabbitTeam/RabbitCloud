using System;

namespace RabbitCloud.Abstractions.Exceptions
{
    public class RabbitFrameworkException : RabbitException
    {
        public RabbitFrameworkException()
        {
        }

        public RabbitFrameworkException(string message) : base(message)
        {
        }

        public RabbitFrameworkException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}