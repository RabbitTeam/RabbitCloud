using System;
using System.Net.Http;

namespace Rabbit.Go
{
    public class GoException : Exception
    {
        public GoException(string message) : base(message)
        {
        }

        public GoException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /*public static GoException ErrorExecuting(HttpRequestMessage request, Exception exception)
        {
            return new RetryableException($"{exception.Message} executing {request.Method} {request.RequestUri}", exception);
        }*/

        public static GoException ErrorExecuting(HttpRequestMessage request, Exception exception)
        {
            return new GoException($"{exception.Message} executing {request.Method} {request.RequestUri}", exception);
        }
    }
}