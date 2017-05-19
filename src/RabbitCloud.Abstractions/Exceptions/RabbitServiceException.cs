using System;

namespace RabbitCloud.Abstractions.Exceptions
{
    public class RabbitServiceException : RabbitException
    {
        public RabbitServiceException()
        {
        }

        public RabbitServiceException(string message) : base(message)
        {
        }

        public RabbitServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}