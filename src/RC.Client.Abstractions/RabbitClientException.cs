using Rabbit.Cloud.Abstractions;
using System;

namespace Rabbit.Cloud.Client.Abstractions
{
    public class RabbitClientException : RabbitException
    {
        public RabbitClientException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public RabbitClientException(string message, int statusCode, Exception innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        public int StatusCode { get; set; }
    }
}