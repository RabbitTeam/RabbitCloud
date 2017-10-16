using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Client.Abstractions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Http
{
    public class RabbitHttpClientHandler : HttpMessageHandler
    {
        private readonly RabbitRequestDelegate _requestDelegate;

        public RabbitHttpClientHandler(RabbitRequestDelegate requestDelegate)
        {
            _requestDelegate = requestDelegate;
        }

        #region Overrides of HttpClientHandler

        /// <inheritdoc />
        /// <summary>Creates an instance of  <see cref="T:System.Net.Http.HttpResponseMessage"></see> based on the information provided in the <see cref="T:System.Net.Http.HttpRequestMessage"></see> as an operation that will not block.</summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="request">request</paramref> was null.</exception>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = new HttpRabbitContext();
            var rabbitRequest = context.Request;

            if (request.Content != null)
            {
                rabbitRequest.Body = await request.Content.ReadAsStreamAsync();
                foreach (var httpContentHeader in request.Content.Headers)
                {
                    rabbitRequest.Headers[httpContentHeader.Key] = new StringValues(httpContentHeader.Value.ToArray());
                }
            }
            rabbitRequest.Method = request.Method;
            rabbitRequest.RequestUri = request.RequestUri;

            await _requestDelegate(context);

            var rabbitResponse = context.Response;

            var response = new HttpResponseMessage
            {
                StatusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), rabbitResponse.StatusCode.ToString()),
                Content = new StreamContent(rabbitResponse.Body)
            };
            foreach (var rabbitResponseHeader in rabbitResponse.Headers)
            {
                response.Content.Headers.Add(rabbitResponseHeader.Key,
                    rabbitResponseHeader.Value.ToArray());
            }

            return response;
        }

        #endregion Overrides of HttpClientHandler
    }
}