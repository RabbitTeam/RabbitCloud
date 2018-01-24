using System;

namespace Rabbit.Go
{
    public class RetryableException : GoException
    {
        public RetryableException(string message) : base(message)
        {
        }

        public RetryableException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}