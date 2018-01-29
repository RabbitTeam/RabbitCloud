using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Application.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Go
{
    public class RabbitCloudMessageHandler : HttpMessageHandler
    {
        private readonly RabbitRequestDelegate _app;

        public RabbitCloudMessageHandler(RabbitRequestDelegate app)
        {
            _app = app;
        }

        #region Overrides of HttpMessageHandler

        /// <summary>Send an HTTP request as an asynchronous operation.</summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="request">request</paramref> was null.</exception>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var rabbitContext = new RabbitContext();

            var rabbitRequest = rabbitContext.Request;

            var requestUri = request.RequestUri;
            rabbitRequest.Host = requestUri.Host;
            //            rabbitRequest.Headers= requestUri.
            rabbitRequest.Port = requestUri.Port;

            var pathAndQuery = requestUri.PathAndQuery;

            var queryStartIndex = pathAndQuery.IndexOf('?');

            if (queryStartIndex != -1)
            {
                rabbitRequest.Path = pathAndQuery.Substring(0, queryStartIndex);
                rabbitRequest.QueryString = pathAndQuery.Substring(queryStartIndex);
            }
            else
                rabbitRequest.Path = pathAndQuery;

            rabbitRequest.Scheme = requestUri.Scheme;

            foreach (var item in request.Headers)
                rabbitRequest.Headers[item.Key] = new StringValues(item.Value.ToArray());

            rabbitRequest.Body = request.Content;

            await _app(rabbitContext);

            return (HttpResponseMessage)rabbitContext.Response.Body;
        }

        #endregion Overrides of HttpMessageHandler
    }
}