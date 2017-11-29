using System;
using System.Runtime.Serialization;

namespace Rabbit.Cloud.Abstractions
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

        protected RabbitException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}