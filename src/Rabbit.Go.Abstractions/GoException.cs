using System;

namespace Rabbit.Go.Abstractions
{
    public class GoException : Exception
    {
        public GoException(string message) : base(message)
        {
        }

        public GoException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}