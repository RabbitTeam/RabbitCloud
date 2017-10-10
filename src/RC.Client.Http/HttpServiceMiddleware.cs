using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Client.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Http
{
    public class HttpServiceMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly ILogger<HttpServiceMiddleware> _logger;

        public HttpServiceMiddleware(RabbitRequestDelegate next, ILogger<HttpServiceMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(RabbitContext context)
        {
            var request = context.Request;
            var requestMessage = new HttpRequestMessage(HttpMethodExtensions.GetHttpMethod(request.Method, HttpMethod.Get), request.RequestUri)
            {
                Content = new StreamContent(request.Body)
            };

            var requestContent = requestMessage.Content;

            foreach (var requestHeader in request.Headers)
            {
                requestContent.Headers.Add(requestHeader.Key, requestHeader.Value.ToArray());
            }

            var httpClient = new HttpClient();
            var httpResponse = await httpClient.SendAsync(requestMessage);

            var responseContent = httpResponse.Content;
            var response = context.Response;
            response.StatusCode = (int)httpResponse.StatusCode;

            if (response.StatusCode >= 500)
            {
                httpResponse.EnsureSuccessStatusCode();
            }

            response.Body = await httpResponse.Content.ReadAsStreamAsync();
            foreach (var httpResponseHeader in responseContent.Headers)
            {
                response.Headers.Add(httpResponseHeader.Key, new StringValues(httpResponseHeader.Value.ToArray()));
            }
        }
    }
}