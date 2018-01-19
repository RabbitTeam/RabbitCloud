using System;
using System.Net.Http;

namespace Rabbit.Cloud.Client.Http
{
    public class HttpRequestDetailedException : HttpRequestException
    {
        public HttpRequestDetailedException(HttpResponseMessage response) : this(response, null)
        {
        }

        public HttpRequestDetailedException(HttpResponseMessage response, Exception innerException)
            : base($"StatusCode: {response.StatusCode}, ReasonPhrase: '{response.ReasonPhrase}', Version: {response.Version}, Headers: {response.Headers}", innerException)
        {
            Response = response;
        }

        public HttpResponseMessage Response { get; }
    }
}