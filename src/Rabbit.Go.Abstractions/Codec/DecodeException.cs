using System;

namespace Rabbit.Go.Abstractions.Codec
{
    public class DecodeException : GoException
    {
        public DecodeException(string message) : base(message)
        {
        }

        public DecodeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}