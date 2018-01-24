using System;

namespace Rabbit.Go.Codec
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