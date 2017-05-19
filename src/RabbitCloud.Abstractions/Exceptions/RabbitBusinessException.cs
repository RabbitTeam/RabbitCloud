using System;

namespace RabbitCloud.Abstractions.Exceptions
{
    public class RabbitBusinessException : RabbitException
    {
        public RabbitBusinessException()
        {
        }

        public RabbitBusinessException(string message) : base(message)
        {
        }

        public RabbitBusinessException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}