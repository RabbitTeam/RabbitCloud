using System;

namespace RabbitCloud.Abstractions.Exceptions
{
    public class RabbitException : Exception
    {
        public RabbitException()
        {
        }

        public RabbitException(string message) : base(message)
        {
        }

        public RabbitException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}